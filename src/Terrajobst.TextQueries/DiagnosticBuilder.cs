using System.Collections.Immutable;

using Terrajobst.TextQueries.Syntax;

namespace Terrajobst.TextQueries;

internal struct DiagnosticBuilder
{
    private ImmutableArray<Diagnostic>.Builder _builder;

    private void Add(Diagnostic diagnostic)
    {
        _builder ??= ImmutableArray.CreateBuilder<Diagnostic>();
        _builder.Add(diagnostic);
    }

    public void AddError(TextSpan span, string message)
    {
        Add(new Diagnostic(span, isError: true, message));
    }

    public void AddWarning(TextSpan span, string message)
    {
        Add(new Diagnostic(span, isError: false, message));
    }

    public ImmutableArray<Diagnostic> ToImmutable()
    {
        return _builder?.ToImmutable() ?? [];
    }
}
