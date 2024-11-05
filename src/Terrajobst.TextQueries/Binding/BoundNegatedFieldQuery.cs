namespace Terrajobst.TextQueries.Binding;

internal sealed class BoundNegatedFieldQuery : BoundQuery
{
    public BoundNegatedFieldQuery(bool isNegated, QueryField field, string value)
    {
        ThrowIfNull(field);
        ThrowIfNull(value);

        IsNegated = isNegated;
        Field = field;
        Value = value;
    }

    public bool IsNegated { get; }

    public new QueryField Field { get; }

    public string Value { get; }
}
