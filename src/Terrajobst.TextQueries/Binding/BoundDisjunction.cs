using System.Collections.Immutable;

namespace Terrajobst.TextQueries.Binding;

internal sealed class BoundDisjunction : BoundQuery
{
    public BoundDisjunction(ImmutableArray<BoundQuery> disjunctions)
    {
        Disjunctions = disjunctions;
    }

    public ImmutableArray<BoundQuery> Disjunctions { get; }
}
