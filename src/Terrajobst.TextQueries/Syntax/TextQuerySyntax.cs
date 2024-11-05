using TheGreatAudit.Data.Querying.Syntax;

namespace Terrajobst.TextQueries.Syntax;

public sealed class TextQuerySyntax : QuerySyntax
{
    internal TextQuerySyntax(QueryToken textToken)
    {
        ThrowIfNull(textToken);

        TextToken = textToken;
    }

    public override QuerySyntaxKind Kind => QuerySyntaxKind.TextQuery;
    public override TextSpan Span => TextToken.Span;
    public QueryToken TextToken { get; }

    public override QueryNodeOrToken[] GetChildren()
    {
        return [TextToken];
    }
}
