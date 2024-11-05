using System.Collections.Immutable;

namespace Terrajobst.TextQueries.Binding;

internal sealed class BoundConjunction : BoundQuery
{
    public BoundConjunction(ImmutableArray<BoundQuery> conjunctions)
    {
        Conjunctions = conjunctions;
    }

    public ImmutableArray<BoundQuery> Conjunctions { get; }
}
