using TheGreatAudit.Data.Querying.Syntax;

namespace Terrajobst.TextQueries.Syntax;

public sealed class NegatedQuerySyntax : QuerySyntax
{
    internal NegatedQuerySyntax(QueryToken notToken, QuerySyntax query)
    {
        ThrowIfNull(notToken);
        ThrowIfNull(query);

        OperatorToken = notToken;
        Query = query;
    }

    public override QuerySyntaxKind Kind => QuerySyntaxKind.NegatedQuery;
    public override TextSpan Span => TextSpan.FromBounds(OperatorToken.Span.Start, Query.Span.End);
    public QueryToken OperatorToken { get; }
    public QuerySyntax Query { get; }

    public override QueryNodeOrToken[] GetChildren()
    {
        return [OperatorToken, Query];
    }
}
