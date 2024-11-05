using System.Text.RegularExpressions;

namespace Terrajobst.TextQueries;

internal readonly partial struct QueryFieldName : IEquatable<QueryFieldName>
{
    private QueryFieldName(string field, string? value)
    {
        ThrowIfNullOrEmpty(field);

        Field = field;
        Value = value;
    }

    public void Deconstruct(out string field, out string? value)
    {
        field = Field;
        value = Value;
    }

    public string Field { get; }

    public string? Value { get; }

    public override bool Equals(object? obj)
    {
        return obj is QueryFieldName name && Equals(name);
    }

    public bool Equals(QueryFieldName other)
    {
        return Field == other.Field &&
               Value == other.Value;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Field, Value);
    }

    public static bool operator ==(QueryFieldName left, QueryFieldName right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(QueryFieldName left, QueryFieldName right)
    {
        return !(left == right);
    }

    public static bool TryParse(string? text, out QueryFieldName result)
    {
        if (string.IsNullOrEmpty(text))
        {
            result = default;
            return false;
        }

        var regex = CreateRegex();
        var match = regex.Match(text);
        if (!match.Success)
        {
            result = default;
            return false;
        }

        var field = match.Groups["Field"].Value;
        var value = match.Groups["Value"].Value;
        if (string.IsNullOrEmpty(value))
            value = null;

        result = new QueryFieldName(field, value);
        return true;
    }

    [GeneratedRegex("(?<Field>[a-zA-Z0-9-]+)(\\:(?<Value>[a-zA-Z0-9-]+))?")]
    private static partial Regex CreateRegex();
}
