using System.Collections.Immutable;

using Terrajobst.TextQueries.Binding;

using TheGreatAudit.Data.Querying.Syntax;

namespace Terrajobst.TextQueries;

public sealed class Query
{
    internal Query(string text,
                   QuerySyntax syntax,
                   BoundQuery boundQuery,
                   ImmutableArray<Diagnostic> diagnostics)
    {
        ThrowIfNull(text);
        ThrowIfNull(syntax);
        ThrowIfNull(boundQuery);

        Text = text;
        Syntax = syntax;
        BoundQuery = boundQuery;
        Diagnostics = diagnostics;
    }

    public string Text { get; }

    public QuerySyntax Syntax { get; }

    public BoundQuery BoundQuery { get; }

    public ImmutableArray<Diagnostic> Diagnostics { get; }

    public override string ToString()
    {
        return Text;
    }
}
