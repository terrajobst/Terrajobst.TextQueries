using System.Collections.Frozen;

namespace Terrajobst.TextQueries;

internal sealed class QueryProcessor<TFieldHandler, TCompletionHandler>
{
    public QueryProcessor(FrozenDictionary<string, QueryField> fields,
                          FrozenDictionary<QueryFieldOrValue, TFieldHandler> handlers,
                          FrozenDictionary<QueryField, TCompletionHandler> completionHandlers)
    {
        ThrowIfNull(fields);
        ThrowIfNull(handlers);

        Fields = fields;
        FieldHandlers = handlers;
        CompletionHandlers = completionHandlers;
    }

    public FrozenDictionary<string, QueryField> Fields { get; }

    public FrozenDictionary<QueryFieldOrValue, TFieldHandler> FieldHandlers { get; }

    public FrozenDictionary<QueryField, TCompletionHandler> CompletionHandlers { get; }
}
