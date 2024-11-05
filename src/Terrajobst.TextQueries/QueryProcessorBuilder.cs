using System.Collections.Frozen;
using System.Reflection;

namespace Terrajobst.TextQueries;

internal abstract class QueryProcessorBuilder<TFieldHandler, TCompletionHandler>
{
    public QueryProcessor<TFieldHandler, TCompletionHandler> Build(Type type)
    {
        var fieldHandlers = GetFieldHandlers(type);
        var fields = fieldHandlers.Keys.OfType<QueryField>()
                                  .Concat(fieldHandlers.Keys.OfType<QueryFieldValue>().Select(v => v.ContainingField).Distinct())
                                  .ToFrozenDictionary(f => f.Name);

        var completionHandlers = GetCompletionHandlers(type, fields);

        return new(fields, fieldHandlers, completionHandlers);
    }

    private FrozenDictionary<QueryFieldOrValue, TFieldHandler> GetFieldHandlers(Type type)
    {
        var descriptors = GetFieldDescriptors(type);
        var fieldGroups = descriptors.GroupBy(d => d.Field)
                                     .OrderBy(g => g.Key);

        var handlerList = new List<KeyValuePair<QueryFieldOrValue, TFieldHandler>>();

        foreach (var fieldGroup in fieldGroups)
        {
            var fieldName = fieldGroup.Key;
            var fieldDescriptors = fieldGroup.OrderBy(d => d.Value).ToArray();

            var dynamicHandlerCount = fieldDescriptors.Count(d => d.Value is null);
            var valueHandlerCount = fieldDescriptors.Count(d => d.Value is not null);

            if (dynamicHandlerCount > 1)
                throw new Exception($"Query field '{fieldName}' cannot have multiple dynamic handlers.");

            if (dynamicHandlerCount == 1 && valueHandlerCount > 0)
                throw new Exception($"Query field '{fieldName}' cannot have both dynamic handlers and fixed value handlers.");

            if (dynamicHandlerCount == 1)
            {
                var descriptor = fieldDescriptors[0];
                var description = descriptor.Description;
                var handler = descriptor.Handler;
                var field = new QueryField(fieldName, description, []);
                handlerList.Add(KeyValuePair.Create((QueryFieldOrValue)field, handler));
            }
            else
            {
                var values = fieldDescriptors.Select(d => (d.Value!, d.Description));
                var field = new QueryField(fieldName, "", values);
                var handlerByValue = fieldDescriptors.ToDictionary(d => d.Value!, d => d.Handler);

                foreach (var fieldValue in field.Values)
                {
                    var handler = handlerByValue[fieldValue.Value];
                    handlerList.Add(KeyValuePair.Create((QueryFieldOrValue)fieldValue, handler));
                }
            }
        }

        var handlers = handlerList.ToFrozenDictionary();
        return handlers;
    }

    private IEnumerable<FieldDescriptor> GetFieldDescriptors(Type type)
    {
        var descriptors = new Dictionary<QueryFieldName, FieldDescriptor>();
        var methods = type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy);

        foreach (var method in methods)
            AddFieldDescriptors(descriptors, method);

        return descriptors.Values;
    }

    private void AddFieldDescriptors(Dictionary<QueryFieldName, FieldDescriptor> descriptors, MethodInfo method)
    {
        var attribute = method.GetCustomAttributesData()
                              .SingleOrDefault(ca => ca.AttributeType == typeof(QueryFieldHandlerAttribute));

        if (attribute is null)
            return;

        if (attribute.ConstructorArguments.Count != 1)
            throw new Exception($"Wrong number of arguments for [{nameof(QueryFieldHandlerAttribute)}] on {method}");

        if (attribute.ConstructorArguments[0].ArgumentType != typeof(string[]))
            throw new Exception($"Wrong type of arguments for [{nameof(QueryFieldHandlerAttribute)}] on {method}");

        var description = "";

        foreach (var named in attribute.NamedArguments)
        {
            if (named.MemberName != nameof(QueryFieldHandlerAttribute.Description))
                throw new Exception($"Unexpected name argument '{named.MemberName}' for [{nameof(QueryFieldHandlerAttribute)}] on {method}");

            if (named.TypedValue.ArgumentType != typeof(string))
                throw new Exception($"Wrong type of named argument '{named.MemberName}' for [{nameof(QueryFieldHandlerAttribute)}] on {method}");

            description = (string?)named.TypedValue.Value ?? "";
        }

        var attributeArguments = (ICollection<CustomAttributeTypedArgument>)attribute.ConstructorArguments[0].Value!;

        if (attributeArguments.Count == 0)
            throw new Exception($"Wrong number of arguments for [{nameof(QueryFieldHandlerAttribute)}] on {method}");

        TFieldHandler handler = GetFieldHandler(method);

        var names = attributeArguments.Select(a => (string)a.Value!);

        foreach (var name in names)
        {
            if (!QueryFieldName.TryParse(name, out var parsedName))
                throw new Exception($"Field name must be of form 'field' or 'field:value'. Actual: '{name}'");

            var descriptor = new FieldDescriptor
            {
                Handler = handler,
                Field = parsedName.Field,
                Value = parsedName.Value,
                Description = description
            };

            if (!descriptors.TryAdd(parsedName, descriptor))
                throw new Exception($"Field name '{name}' is not unique");
        }
    }

    protected abstract TFieldHandler GetFieldHandler(MethodInfo method);

    private FrozenDictionary<QueryField, TCompletionHandler> GetCompletionHandlers(Type type, FrozenDictionary<string, QueryField> fields)
    {
        var handlers = new Dictionary<QueryField, TCompletionHandler>();
        var methods = type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy);

        foreach (var method in methods)
        {
            var attribute = method.GetCustomAttributesData()
                                  .SingleOrDefault(ca => ca.AttributeType == typeof(QueryFieldCompletionHandlerAttribute));

            if (attribute is null)
                continue;

            if (attribute.ConstructorArguments.Count != 1)
                throw new Exception($"Wrong number of arguments for [{nameof(QueryFieldCompletionHandlerAttribute)}] on {method}");

            if (attribute.ConstructorArguments[0].ArgumentType != typeof(string[]))
                throw new Exception($"Wrong type of arguments for [{nameof(QueryFieldCompletionHandlerAttribute)}] on {method}");

            var attributeArguments = (ICollection<CustomAttributeTypedArgument>)attribute.ConstructorArguments[0].Value!;

            if (attributeArguments.Count == 0)
                throw new Exception($"Wrong number of arguments for [{nameof(QueryFieldCompletionHandlerAttribute)}] on {method}");

            var handler = GetCompletionHandler(method);

            var names = attributeArguments.Select(a => (string)a.Value!);

            foreach (var name in names)
            {
                if (!fields.TryGetValue(name, out var field))
                    throw new Exception($"Query field '{name}' does not exist");

                if (!handlers.TryAdd(field, handler))
                    throw new Exception($"Query field '{name}' has multiple completion handlers");
            }
        }

        return handlers.ToFrozenDictionary();
    }

    protected abstract TCompletionHandler GetCompletionHandler(MethodInfo method);

    private sealed class FieldDescriptor
    {
        public required string Field { get; init; }
        public string? Value { get; init; }
        public required string Description { get; init; }
        public required TFieldHandler Handler { get; init; }
    }
}
