namespace Terrajobst.TextQueries.Binding;

public sealed class BoundFieldQuery : BoundQuery
{
    internal BoundFieldQuery(QueryField field, string value)
    {
        ThrowIfNull(field);
        ThrowIfNull(value);

        Field = field;
        Value = value;
    }

    public new QueryField Field { get; }

    public string Value { get; }
}
