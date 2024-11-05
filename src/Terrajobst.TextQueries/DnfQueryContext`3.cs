using System.Collections.Frozen;
using System.Collections.Immutable;

using Terrajobst.TextQueries.Binding;
using Terrajobst.TextQueries.Completion;

using TheGreatAudit.Data.Querying.Syntax;

namespace Terrajobst.TextQueries;

public abstract partial class DnfQueryContext<TContext, TDisjunction, TConjunction> : QueryContext<TContext, TDisjunction>
{
    private readonly FrozenDictionary<string, QueryField> _fields;
    private readonly FrozenDictionary<QueryFieldOrValue, Action<TContext, TConjunction, bool, string>> _fieldHandlers;
    private readonly FrozenDictionary<QueryField, Func<TContext, IEnumerable<string>>> _completionHandlers;

    protected DnfQueryContext()
    {
        var handler = HandlerBuilder.Default.Build(GetType());

        _fields = handler.Fields;
        _fieldHandlers = handler.FieldHandlers;
        _completionHandlers = handler.CompletionHandlers;
    }

    private protected override BoundQuery BindQuery(QuerySyntax syntax, out ImmutableArray<Diagnostic> diagnostics)
    {
        var boundQuery = Binder.Bind(syntax, _fields, out diagnostics);
        return Binder.ToDisjunctiveNormalForm(boundQuery);
    }

    private protected override TDisjunction GenerateQuery(TContext context, BoundQuery query)
    {
        if (query is not BoundDisjunction n)
            throw new Exception($"Unexpected query {query.GetType()}");

        var conjugations = ImmutableArray.CreateBuilder<TConjunction>(n.Disjunctions.Length);

        foreach (var conjunction in n.Disjunctions)
        {
            var conjunctionResult = GenerateConjunction(context);
            conjugations.Add(conjunctionResult);

            Apply(context, conjunctionResult, conjunction);
        }

        return GenerateDisjunction(context, conjugations.ToImmutable());
    }

    private protected override QueryCompletionProvider CreateCompletionProvider(TContext context)
    {
        return new QueryCompletionProvider<TContext>(context, _fields, _completionHandlers);
    }

    private void Apply(TContext context, TConjunction conjunctionResult, BoundQuery query)
    {
        switch (query)
        {
            case BoundNegatedFieldQuery n:
                ApplyFieldQuery(context, conjunctionResult, n);
                break;
            case BoundNegatedFieldValueQuery n:
                ApplyFieldValueQuery(context, conjunctionResult, n);
                break;
            case BoundNegatedTextQuery n:
                ApplyTextQuery(context, conjunctionResult, n);
                break;
            case BoundConjunction n:
                ApplyConjunction(context, conjunctionResult, n);
                break;
            default:
                throw new Exception($"Unexpected query {query.GetType()}");
        }
    }

    private void ApplyFieldQuery(TContext context, TConjunction conjunction, BoundNegatedFieldQuery query)
    {
        var handler = _fieldHandlers[query.Field];
        handler(context, conjunction, query.IsNegated, query.Value);
    }

    private void ApplyFieldValueQuery(TContext context, TConjunction conjunction, BoundNegatedFieldValueQuery query)
    {
        var handler = _fieldHandlers[query.Value];
        handler(context, conjunction, query.IsNegated, query.Value.Value);
    }

    private void ApplyTextQuery(TContext context, TConjunction conjunction, BoundNegatedTextQuery query)
    {
        ApplyText(context, conjunction, query.IsNegated, query.Text);
    }

    private void ApplyConjunction(TContext context, TConjunction conjunction, BoundConjunction query)
    {
        foreach (var conjunctionQuery in query.Conjunctions)
            Apply(context, conjunction, conjunctionQuery);
    }

    protected abstract TDisjunction GenerateDisjunction(TContext context, ImmutableArray<TConjunction> conjunctions);

    protected abstract TConjunction GenerateConjunction(TContext context);

    protected abstract void ApplyText(TContext context, TConjunction conjunction, bool isNegated, string text);
}
