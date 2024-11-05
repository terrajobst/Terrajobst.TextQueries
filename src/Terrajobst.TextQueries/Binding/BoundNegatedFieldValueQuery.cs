namespace Terrajobst.TextQueries.Binding;

internal sealed class BoundNegatedFieldValueQuery : BoundQuery
{
    public BoundNegatedFieldValueQuery(bool isNegated, QueryFieldValue value)
    {
        ThrowIfNull(value);

        IsNegated = isNegated;
        Value = value;
    }

    public bool IsNegated { get; }

    public new QueryField Field => Value.ContainingField;

    public QueryFieldValue Value { get; }
}
