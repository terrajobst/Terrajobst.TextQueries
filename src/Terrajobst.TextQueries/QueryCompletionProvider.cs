using System.Diagnostics;

using Terrajobst.TextQueries.Syntax;

using TheGreatAudit.Data.Querying.Syntax;

namespace Terrajobst.TextQueries.Completion;

internal abstract class QueryCompletionProvider
{
    public static QueryCompletionProvider Empty { get; } = new EmptyQueryCompletionProvider();

    public QueryCompletionResult Complete(QuerySyntax node, int position)
    {
        ThrowIfNull(node);
        ThrowIfNegative(position);

        var query = FindLeafQuery(node, position);

        if (query is TextQuerySyntax text)
            return GetTextCompletions(text);

        if (query is KeyValueQuerySyntax keyValue)
            return GetKeyValueCompletions(keyValue, position);

        return GetKeywordCompletions(position);
    }

    private static QuerySyntax? FindLeafQuery(QuerySyntax node, int position)
    {
        var contained = node.Span.Start <= position &&
                        position <= node.Span.End;

        if (!contained)
            return null;

        var children = node.GetChildren();

        for (var i = 0; i < children.Length; i++)
        {
            var child = children[i];
            var start = child.Span.Start;
            var end = child.Span.End;

            if (start <= position && position <= end)
            {
                if (child is QuerySyntax query)
                    return FindLeafQuery(query, position);
                else
                    return node;
            }
        }

        return node;
    }

    private QueryCompletionResult GetTextCompletions(TextQuerySyntax text)
    {
        Debug.Assert(text.TextToken.Value is not null);

        var completions = GetCompletionsForText(text.TextToken.Value);
        return new QueryCompletionResult(completions, text.TextToken.Span);
    }

    private QueryCompletionResult GetKeyValueCompletions(KeyValueQuerySyntax keyValue, int position)
    {
        Debug.Assert(keyValue.KeyToken.Value is not null);

        if (position < keyValue.ColonToken.Span.End)
        {
            var completions = GetCompletionsForText(keyValue.KeyToken.Value);
            return new QueryCompletionResult(completions, keyValue.KeyToken.Span);
        }
        else
        {
            Debug.Assert(keyValue.ValueToken.Value is not null);
            var completions = GetCompletionForKeyValue(keyValue.KeyToken.Value, keyValue.ValueToken.Value);
            return new QueryCompletionResult(completions, keyValue.ValueToken.Span);
        }
    }

    private QueryCompletionResult GetKeywordCompletions(int position)
    {
        var completions = GetCompletionsForText(string.Empty);
        return new QueryCompletionResult(completions, TextSpan.FromBounds(position, position));
    }

    private IEnumerable<string> GetCompletionForKeyValue(string key, string value)
    {
        var items = GetFieldValues(key);
        var completions = items.Distinct().Order().ToArray();

        var index = Array.BinarySearch(completions, value, StringComparer.OrdinalIgnoreCase);
        if (index < 0)
            index = ~index;

        foreach (var item in completions.Skip(index))
        {
            if (!item.StartsWith(value, StringComparison.OrdinalIgnoreCase))
                yield break;

            yield return Escape(item) + " ";
        }
    }

    private IEnumerable<string> GetCompletionsForText(string text)
    {
        var operators = new[] { "(", ")", "-", "not ", "or " };
        var keywords = operators
                        .Concat(GetFields().Select(k => k + ":"))
                        .Order()
                        .ToArray();

        var index = Array.BinarySearch(keywords, text, StringComparer.OrdinalIgnoreCase);
        if (index < 0)
            index = ~index;

        foreach (var keyword in keywords.Skip(index))
        {
            if (!keyword.StartsWith(text, StringComparison.OrdinalIgnoreCase))
                yield break;

            yield return keyword;
        }
    }

    protected virtual IEnumerable<string> GetFields() => [];

    protected virtual IEnumerable<string> GetFieldValues(string field) => [];

    private static string Escape(string text)
    {
        if (!text.Contains(" "))
            return text;

        return $"\"{text}\"";
    }

    private sealed class EmptyQueryCompletionProvider : QueryCompletionProvider
    {
    }
}
