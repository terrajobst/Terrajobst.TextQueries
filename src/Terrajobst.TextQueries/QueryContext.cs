using System.Collections.Frozen;
using System.Reflection;

using Terrajobst.TextQueries.Binding;

using TheGreatAudit.Data.Querying.Syntax;

using Binder = Terrajobst.TextQueries.Binding.Binder;

namespace Terrajobst.TextQueries;

public abstract class QueryContext<TContext, TNode>
{
    private readonly FrozenDictionary<string, QueryField> _fields;
    private readonly FrozenDictionary<QueryFieldOrValue, Func<TContext, string, TNode>> _handlers;

    protected QueryContext()
    {
        _handlers = CreateHandlers(GetType());
        _fields = _handlers.Keys.OfType<QueryField>().ToFrozenDictionary(f => f.Name);
    }

    private static FrozenDictionary<QueryFieldOrValue, Func<TContext, string, TNode>> CreateHandlers(Type type)
    {
        var descriptors = GetDescriptors(type);

        var fieldGroups = descriptors.GroupBy(d => d.Field);
        var result = new List<KeyValuePair<QueryFieldOrValue, Func<TContext, string, TNode>>>();

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
                result.Add(KeyValuePair.Create((QueryFieldOrValue)field, handler));
            }
            else
            {
                var values = fieldDescriptors.Select(d => (d.Value!, d.Description));
                var field = new QueryField(fieldName, "", values);
                var handlerByValue = fieldDescriptors.ToDictionary(d => d.Value!, d => d.Handler);
                
                foreach (var fieldValue in field.Values)
                {
                    var handler = handlerByValue[fieldValue.Value];
                    result.Add(KeyValuePair.Create((QueryFieldOrValue)fieldValue, handler));
                }
            }
        }

        return result.ToFrozenDictionary();
    }

    private static List<HandlerDescriptor> GetDescriptors(Type type)
    {
        var descriptors = new List<HandlerDescriptor>();
        var methods = type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy);

        foreach (var method in methods)
            AddDescriptors(descriptors, method);
        return descriptors;
    }

    private static void AddDescriptors(List<HandlerDescriptor> descriptors, MethodInfo method)
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

        var parameters = method.GetParameters();

        Func<TContext, string, TNode> handler;

        // Allowed signatures:
        //
        // TNode M()
        // TNode M(string value)
        // TNode M(TContext context)
        // TNode M(TContext context, string value)

        if (method.ReturnType != typeof(TNode))
            throw new Exception($"Wrong return type for {method}. Expected: {typeof(TNode).Name}");

        if (parameters.Length == 0)
        {
            handler = (context, value) => (TNode)method.Invoke(null, null)!;
        }
        else if (parameters.Length == 1 && parameters[0].ParameterType == typeof(string))
        {
            handler = (context, value) => (TNode)method.Invoke(null, [value])!;
        }
        else if (parameters.Length == 1 && parameters[0].ParameterType == typeof(TContext))
        {
            handler = (context, value) => (TNode)method.Invoke(null, [context])!;
        }
        else if (parameters.Length == 2 && parameters[0].ParameterType == typeof(TContext) &&
                                           parameters[1].ParameterType == typeof(string))
        {
            handler = (context, value) => (TNode)method.Invoke(null, [context, value])!;
        }
        else
        {
            throw new Exception($"Unexpected signature for {method}");
        }

        // TODO: Validate string format is either 'field' or 'field:value'
        // TODO: Ensure kv is unique

        var strings = attributeArguments.Select(a => (string)a.Value!);
        var pairs = GetKeyValues(strings).ToArray();

        foreach (var (k, v) in pairs)
        {
            var descriptor = new HandlerDescriptor
            {
                Handler = handler,
                Field = k,
                Value = v,
                Description = description
            };

            descriptors.Add(descriptor);
        }
    }

    private sealed class HandlerDescriptor
    {
        public required string Field { get; init; }
        public string? Value { get; init; }
        public required string Description { get; init; }
        public required Func<TContext, string, TNode> Handler { get; init; }
    }

    private static IEnumerable<(string Key, string? Value)> GetKeyValues(IEnumerable<string> pairs)
    {
        foreach (var pair in pairs)
        {
            var kv = pair.Split(":");
            if (kv.Length == 1)
                yield return (kv[0], null);
            else if (kv.Length == 2)
                yield return (kv[0], kv[1]);
            else
                throw new ArgumentException($"Invalid syntax: '{pair}'", nameof(pairs));
        }
    }

    public Query CreateQuery(string text)
    {
        ThrowIfNull(text);

        var syntax = QuerySyntax.Parse(text, out var syntaxDiagnostics);
        var boundQuery = Binder.Bind(syntax, _fields, out var bindingDiagnostics);
        var diagnostics = syntaxDiagnostics.AddRange(bindingDiagnostics);
        return new Query(text, syntax, boundQuery, diagnostics);
    }

    public TNode GenerateNode(Query query, TContext context)
    {
        ThrowIfNull(query);
        
        return GenerateNode(context, query.BoundQuery);
    }

    private TNode GenerateNode(TContext context, BoundQuery query)
    {
        switch (query)
        {
            case BoundFieldQuery n:
                return GenerateField(context, n);
            case BoundFieldValueQuery n:
                return GenerateFieldValue(context, n);
            case BoundTextQuery n:
                return GenerateText(context, n);
            case BoundNegatedQuery n:
                return GenerateNegated(context, n);
            case BoundAndQuery n:
                return GenerateAnd(context, n);
            case BoundOrQuery n:
                return GenerateOr(context, n);
            default:
                throw new Exception($"Unexpected query {query.GetType()}");
        }
    }

    private TNode GenerateField(TContext context, BoundFieldQuery query)
    {
        var handler = _handlers[query.Field];
        return handler(context, query.Value);
    }

    private TNode GenerateFieldValue(TContext context, BoundFieldValueQuery query)
    {
        var handler = _handlers[query.Value];
        return handler(context, query.Value.Value);
    }

    private TNode GenerateText(TContext context, BoundTextQuery query)
    {
        return GenerateText(context, query.Text);
    }

    private TNode GenerateNegated(TContext context, BoundNegatedQuery query)
    {
        var argument = GenerateNode(context, query.Query);
        return GenerateNegated(context, argument);
    }

    private TNode GenerateAnd(TContext context, BoundAndQuery query)
    {
        var left = GenerateNode(context, query.Left);
        var right = GenerateNode(context, query.Left);
        return GenerateAnd(context, left, right);
    }

    private TNode GenerateOr(TContext context, BoundOrQuery query)
    {
        var left = GenerateNode(context, query.Left);
        var right = GenerateNode(context, query.Left);
        return GenerateOr(context, left, right);
    }

    protected abstract TNode GenerateText(TContext context, string text);

    protected abstract TNode GenerateNegated(TContext context, TNode argument);

    protected abstract TNode GenerateAnd(TContext context, TNode left, TNode right);

    protected abstract TNode GenerateOr(TContext context, TNode left, TNode right);
}
