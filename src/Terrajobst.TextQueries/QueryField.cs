using System.Collections.Immutable;

namespace Terrajobst.TextQueries;

public sealed class QueryField : QueryFieldOrValue
{
    internal QueryField(string name, string description, IEnumerable<(string Value, string Description)> values)
        : base(name, description)
    {
        ThrowIfNull(values);

        Values = values.Select(t =>
        {
            var valueName = $"{name}:{t.Value}";
            return new QueryFieldValue(this, valueName, t.Value, t.Description);
        }).ToImmutableArray();
    }

    public ImmutableArray<QueryFieldValue> Values { get; }
}
