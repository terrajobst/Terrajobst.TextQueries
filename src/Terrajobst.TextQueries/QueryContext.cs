using System.Collections.Immutable;

using Terrajobst.TextQueries.Binding;

using TheGreatAudit.Data.Querying.Syntax;

namespace Terrajobst.TextQueries;

public abstract class QueryContext
{
    private protected QueryContext()
    {
    }

    public Query CreateQuery(string text)
    {
        ThrowIfNull(text);

        var syntax = QuerySyntax.Parse(text, out var syntaxDiagnostics);
        var boundQuery = BindQuery(syntax, out var bindingDiagnostics);
        var diagnostics = syntaxDiagnostics.AddRange(bindingDiagnostics);
        return new Query(this, text, syntax, boundQuery, diagnostics);
    }

    private protected abstract BoundQuery BindQuery(QuerySyntax syntax, out ImmutableArray<Diagnostic> diagnostics);
}
