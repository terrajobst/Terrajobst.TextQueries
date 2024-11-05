namespace Terrajobst.TextQueries.Binding;

public sealed class BoundFieldValueQuery : BoundQuery
{
    internal BoundFieldValueQuery(QueryFieldValue value)
    {
        ThrowIfNull(value);

        Value = value;
    }

    public new QueryField Field => Value.ContainingField;

    public QueryFieldValue Value { get; }
}
