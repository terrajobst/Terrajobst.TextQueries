using System.Collections.Frozen;
using System.Collections.Immutable;

using Terrajobst.TextQueries.Syntax;

using TheGreatAudit.Data.Querying.Syntax;

namespace Terrajobst.TextQueries.Binding;

internal sealed class Binder
{
    public static BoundQuery Bind(QuerySyntax syntax,
                                  FrozenDictionary<string, QueryField> fields,
                                  out ImmutableArray<Diagnostic> diagnostics)
    {
        ThrowIfNull(syntax);
        ThrowIfNull(fields);

        var binder = new Binder(fields);
        var result = binder.Bind(syntax);
        diagnostics = binder.GetDiagnostics();
        return result;
    }

    private readonly FrozenDictionary<string, QueryField> _fields;
    private DiagnosticBuilder _diagnosticBuilder;

    private Binder(FrozenDictionary<string, QueryField> fields)
    {
        _fields = fields;
    }

    public ImmutableArray<Diagnostic> GetDiagnostics() => _diagnosticBuilder.ToImmutable();

    private BoundQuery Bind(QuerySyntax syntax)
    {
        switch (syntax.Kind)
        {
            case QuerySyntaxKind.TextQuery:
                return BindTextExpression((TextQuerySyntax)syntax);
            case QuerySyntaxKind.KeyValueQuery:
                return BindKeyValueExpression((KeyValueQuerySyntax)syntax);
            case QuerySyntaxKind.NegatedQuery:
                return BindNegatedExpression((NegatedQuerySyntax)syntax);
            case QuerySyntaxKind.AndQuery:
                return BindAndExpression((AndQuerySyntax)syntax);
            case QuerySyntaxKind.OrQuery:
                return BindOrExpression((OrQuerySyntax)syntax);
            case QuerySyntaxKind.ParenthesizedQuery:
                return BindParenthesizedExpression((ParenthesizedQuerySyntax)syntax);
            default:
                throw new Exception($"Unexpected node {syntax.Kind}");
        }
    }

    private BoundQuery BindTextExpression(TextQuerySyntax node)
    {
        return BoundQuery.Text(node.TextToken.Value!);
    }

    private BoundQuery BindKeyValueExpression(KeyValueQuerySyntax node)
    {
        var name = node.KeyToken.Value!;
        var value = node.ValueToken.Value!;

        if (!_fields.TryGetValue(name, out var field))
        {
            _diagnosticBuilder.AddError(node.KeyToken.Span, $"Unknown field '{name}'");
            return BoundQuery.Text($"{name}:{value}");
        }

        if (!field.Values.Any())
            return BoundQuery.Field(field, value);

        var fieldValue = field.Values.FirstOrDefault(v => v.Value == value);
        if (fieldValue is null)
        {
            _diagnosticBuilder.AddError(node.ValueToken.Span, $"Unknown value '{value}' for field '{name}'");
            return BoundQuery.Text($"{name}:{value}");
        }

        return BoundQuery.FieldValue(fieldValue);
    }

    private BoundQuery BindNegatedExpression(NegatedQuerySyntax node)
    {
        return BoundQuery.Negate(Bind(node.Query));
    }

    private BoundQuery BindAndExpression(AndQuerySyntax node)
    {
        return BoundQuery.And(Bind(node.Left), Bind(node.Right));
    }

    private BoundQuery BindOrExpression(OrQuerySyntax node)
    {
        return BoundQuery.Or(Bind(node.Left), Bind(node.Right));
    }

    private BoundQuery BindParenthesizedExpression(ParenthesizedQuerySyntax node)
    {
        return Bind(node.Query);
    }
}