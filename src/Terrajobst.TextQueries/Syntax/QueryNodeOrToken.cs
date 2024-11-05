namespace Terrajobst.TextQueries.Syntax;

public abstract class QueryNodeOrToken
{
    private protected QueryNodeOrToken()
    {
    }

    public abstract QuerySyntaxKind Kind { get; }
    public abstract TextSpan Span { get; }
    public abstract QueryNodeOrToken[] GetChildren();
}
