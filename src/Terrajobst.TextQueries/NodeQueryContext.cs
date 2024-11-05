using System.Collections.Frozen;
using System.Collections.Immutable;

using Terrajobst.TextQueries.Binding;
using Terrajobst.TextQueries.Completion;

using TheGreatAudit.Data.Querying.Syntax;

namespace Terrajobst.TextQueries;

public abstract partial class NodeQueryContext<TContext, TNode> : QueryContext<TContext, TNode>
{
    private readonly FrozenDictionary<string, QueryField> _fields;
    private readonly FrozenDictionary<QueryFieldOrValue, Func<TContext, string, TNode>> _fieldHandlers;
    private readonly FrozenDictionary<QueryField, Func<TContext, IEnumerable<string>>> _completionHandlers;

    protected NodeQueryContext()
    {
        var handler = HandlerBuilder.Default.Build(GetType());

        _fields = handler.Fields;
        _fieldHandlers = handler.FieldHandlers;
        _completionHandlers = handler.CompletionHandlers;
    }

    private protected override BoundQuery BindQuery(QuerySyntax syntax, out ImmutableArray<Diagnostic> diagnostics)
    {
        return Binder.Bind(syntax, _fields, out diagnostics);
    }

    private protected override TNode GenerateQuery(TContext context, BoundQuery query)
    {
        return GenerateNode(context, query);
    }

    private protected override QueryCompletionProvider CreateCompletionProvider(TContext context)
    {
        return new QueryCompletionProvider<TContext>(context, _fields, _completionHandlers);
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
        var handler = _fieldHandlers[query.Field];
        return handler(context, query.Value);
    }

    private TNode GenerateFieldValue(TContext context, BoundFieldValueQuery query)
    {
        var handler = _fieldHandlers[query.Value];
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
        var right = GenerateNode(context, query.Right);
        return GenerateAnd(context, left, right);
    }

    private TNode GenerateOr(TContext context, BoundOrQuery query)
    {
        var left = GenerateNode(context, query.Left);
        var right = GenerateNode(context, query.Right);
        return GenerateOr(context, left, right);
    }

    protected abstract TNode GenerateText(TContext context, string text);

    protected abstract TNode GenerateNegated(TContext context, TNode argument);

    protected abstract TNode GenerateAnd(TContext context, TNode left, TNode right);

    protected abstract TNode GenerateOr(TContext context, TNode left, TNode right);
}
