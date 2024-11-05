namespace Terrajobst.TextQueries;

public sealed class QueryFieldValue : QueryFieldOrValue
{
    internal QueryFieldValue(QueryField containingField, string name, string value, string description)
        : base(name, description)
    {
        ThrowIfNull(containingField);
        ThrowIfNull(value);

        ContainingField = containingField;
        Value = value;
    }

    public QueryField ContainingField { get; }

    public string Value { get; }
}
