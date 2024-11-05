using System.Collections.Immutable;

using Terrajobst.TextQueries.Completion;

namespace Terrajobst.TextQueries;

public abstract class DnfQueryContext<TDisjunction, TConjunction> : DnfQueryContext<object?, TDisjunction, TConjunction>
{
    protected override TDisjunction GenerateDisjunction(object? context, ImmutableArray<TConjunction> conjunctions)
    {
        return GenerateDisjunction(conjunctions);
    }

    protected override TConjunction GenerateConjunction(object? context)
    {
        return GenerateConjunction();
    }

    protected override void ApplyText(object? context, TConjunction conjunction, bool isNegated, string text)
    {
        ApplyText(conjunction, isNegated, text);
    }

    protected abstract TDisjunction GenerateDisjunction(ImmutableArray<TConjunction> conjunctions);

    protected abstract TConjunction GenerateConjunction();

    protected abstract void ApplyText(TConjunction conjunction, bool isNegated, string text);

    public QueryCompletionResult Complete(string text, int position)
    {
        return Complete(text, position, null);
    }

    public QueryCompletionResult Complete(Query query, int position)
    {
        return Complete(query, position, null);
    }
}
