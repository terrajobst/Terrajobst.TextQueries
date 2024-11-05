using System.Collections.Frozen;

namespace Terrajobst.TextQueries.Completion;

internal sealed class QueryCompletionProvider<TContext> : QueryCompletionProvider
{
    private readonly TContext _context;
    private readonly FrozenDictionary<string, QueryField> _fields;
    private readonly FrozenDictionary<QueryField, Func<TContext, IEnumerable<string>>> _completionHandlers;

    public QueryCompletionProvider(TContext context,
                                   FrozenDictionary<string, QueryField> fields,
                                   FrozenDictionary<QueryField, Func<TContext, IEnumerable<string>>> completionHandlers)
    {
        ThrowIfNull(fields);
        ThrowIfNull(completionHandlers);

        _context = context;
        _fields = fields;
        _completionHandlers = completionHandlers;
    }

    protected override IEnumerable<string> GetFields()
    {
        return _fields.Values.Select(f => f.Name).Order();
    }

    protected override IEnumerable<string> GetFieldValues(string field)
    {
        if (_fields.TryGetValue(field, out var f))
        {
            if (f.Values.Any())
                return f.Values.Select(v => v.Value);

            if (_completionHandlers.TryGetValue(f, out var completionHandler))
                return completionHandler(_context);
        }

        return [];
    }
}