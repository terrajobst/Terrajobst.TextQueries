namespace Terrajobst.TextQueries.Binding;

internal sealed class BoundFieldQuery : BoundQuery
{
    public BoundFieldQuery(QueryField field, string value)
    {
        ThrowIfNull(field);
        ThrowIfNull(value);

        Field = field;
        Value = value;
    }

    public new QueryField Field { get; }

    public string Value { get; }
}
