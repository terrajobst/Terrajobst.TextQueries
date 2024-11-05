using Terrajobst.TextQueries.Syntax;

namespace Terrajobst.TextQueries.Completion;

public sealed class QueryCompletionResult
{
    internal QueryCompletionResult(IEnumerable<string> completions, TextSpan span)
    {
        Completions = completions;
        Span = span;
    }

    public IEnumerable<string> Completions { get; }
    public TextSpan Span { get; }
}
