using System.Collections.Immutable;

using Terrajobst.TextQueries.Binding;

using TheGreatAudit.Data.Querying.Syntax;

namespace Terrajobst.TextQueries;

public sealed class Query
{
    internal Query(QueryContext context,
                   string text,
                   QuerySyntax syntax,
                   BoundQuery boundQuery,
                   ImmutableArray<Diagnostic> diagnostics)
    {
        ThrowIfNull(context);
        ThrowIfNull(text);
        ThrowIfNull(syntax);
        ThrowIfNull(boundQuery);

        Context = context;
        Text = text;
        Syntax = syntax;
        BoundQuery = boundQuery;
        Diagnostics = diagnostics;
    }

    public QueryContext Context { get; }

    public string Text { get; }

    public QuerySyntax Syntax { get; }

    internal BoundQuery BoundQuery { get; }

    public ImmutableArray<Diagnostic> Diagnostics { get; }

    public override string ToString()
    {
        return Text;
    }
}
