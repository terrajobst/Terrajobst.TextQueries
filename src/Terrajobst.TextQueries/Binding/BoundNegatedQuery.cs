namespace Terrajobst.TextQueries.Binding;

public sealed class BoundNegatedQuery : BoundQuery
{
    internal BoundNegatedQuery(BoundQuery query)
    {
        ThrowIfNull(query);

        Query = query;
    }

    public BoundQuery Query { get; }
}
