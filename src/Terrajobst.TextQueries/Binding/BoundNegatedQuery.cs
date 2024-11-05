namespace Terrajobst.TextQueries.Binding;

internal sealed class BoundNegatedQuery : BoundQuery
{
    public BoundNegatedQuery(BoundQuery query)
    {
        ThrowIfNull(query);

        Query = query;
    }

    public BoundQuery Query { get; }
}
