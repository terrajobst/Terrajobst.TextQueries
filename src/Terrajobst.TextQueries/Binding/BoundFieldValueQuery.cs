namespace Terrajobst.TextQueries.Binding;

internal sealed class BoundFieldValueQuery : BoundQuery
{
    public BoundFieldValueQuery(QueryFieldValue value)
    {
        ThrowIfNull(value);

        Value = value;
    }

    public new QueryField Field => Value.ContainingField;

    public QueryFieldValue Value { get; }
}
