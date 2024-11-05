namespace Terrajobst.TextQueries.Syntax;

public enum QuerySyntaxKind
{
    None,
    EndOfFile,
    WhitespaceToken,
    TextToken,
    QuotedTextToken,
    OpenParenthesisToken,
    CloseParenthesisToken,
    ColonToken,

    OrKeyword,
    NotKeyword,

    TextQuery,
    KeyValueQuery,
    OrQuery,
    AndQuery,
    NegatedQuery,
    ParenthesizedQuery,
}
