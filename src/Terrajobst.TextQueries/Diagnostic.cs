using Terrajobst.TextQueries.Syntax;

namespace Terrajobst.TextQueries;

public sealed class Diagnostic
{
    internal Diagnostic(TextSpan span, bool isError, string message)
    {
        ThrowIfNullOrEmpty(message);

        Span = span;
        IsError = isError;
        Message = message;
    }

    public TextSpan Span { get; }
    public bool IsError { get; }
    public string Message { get; }
}
