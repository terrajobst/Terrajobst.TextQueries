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

    // DNF

    public static BoundDisjunction ToDisjunctiveNormalForm(BoundQuery query)
    {
        var dnf = ToDisjunctiveNormalFormUnflattened(query);
        return CreateDisjunction(dnf);
    }

    private static BoundQuery ToDisjunctiveNormalFormUnflattened(BoundQuery query)
    {
        switch (query)
        {
            case BoundFieldQuery q:
                return new BoundNegatedFieldQuery(false, q.Field, q.Value);
            case BoundFieldValueQuery q:
                return new BoundNegatedFieldValueQuery(false, q.Value);
            case BoundTextQuery q:
                return new BoundNegatedTextQuery(false, q.Text);
            case BoundNegatedQuery q:
                return ToDisjunctiveNormalFormUnflattened(Negate(q.Query));
            case BoundNegatedFieldQuery:
            case BoundNegatedFieldValueQuery:
            case BoundNegatedTextQuery:
                return query;
            case BoundOrQuery q:
            {
                var left = ToDisjunctiveNormalFormUnflattened(q.Left);
                var right = ToDisjunctiveNormalFormUnflattened(q.Right);
                if (ReferenceEquals(left, q.Left) &&
                    ReferenceEquals(right, q.Right))
                    return query;

                return new BoundOrQuery(left, right);
            }
            case BoundAndQuery q:
            {
                var left = ToDisjunctiveNormalFormUnflattened(q.Left);
                var right = ToDisjunctiveNormalFormUnflattened(q.Right);

                // (A OR B) AND C      ->    (A AND C) OR (B AND C)

                if (left is BoundOrQuery leftOr)
                {
                    var a = leftOr.Left;
                    var b = leftOr.Right;
                    var c = right;
                    return new BoundOrQuery(
                        ToDisjunctiveNormalFormUnflattened(new BoundAndQuery(a, c)),
                        ToDisjunctiveNormalFormUnflattened(new BoundAndQuery(b, c))
                    );
                }

                // A AND (B OR C)      ->    (A AND B) OR (A AND C)

                if (right is BoundOrQuery rightOr)
                {
                    var a = left;
                    var b = rightOr.Left;
                    var c = rightOr.Right;
                    return new BoundOrQuery(
                        ToDisjunctiveNormalFormUnflattened(new BoundAndQuery(a, b)),
                        ToDisjunctiveNormalFormUnflattened(new BoundAndQuery(a, c))
                    );
                }

                return new BoundAndQuery(left, right);
            }
            default:
                throw new Exception($"Unknown query {query.GetType()}");
        }
    }

    private static BoundQuery Negate(BoundQuery node)
    {
        switch (node)
        {
            case BoundFieldQuery q:
                return NegateField(q);
            case BoundFieldValueQuery q:
                return NegateFieldValue(q);
            case BoundTextQuery q:
                return NegateText(q);
            case BoundNegatedFieldQuery q:
                return NegateField(q);
            case BoundNegatedFieldValueQuery q:
                return NegateFieldValue(q);
            case BoundNegatedTextQuery q:
                return NegateText(q);
            case BoundNegatedQuery q:
                return NegateNegatedQuery(q);
            case BoundAndQuery q:
                return NegateAnd(q);
            case BoundOrQuery q:
                return NegateOr(q);
            default:
                throw new Exception($"Unexpected node {node.GetType()}");
        }
    }

    private static BoundQuery NegateField(BoundFieldQuery node)
    {
        return new BoundNegatedFieldQuery(true, node.Field, node.Value);
    }

    private static BoundQuery NegateFieldValue(BoundFieldValueQuery node)
    {
        return new BoundNegatedFieldValueQuery(true, node.Value);
    }

    private static BoundQuery NegateText(BoundTextQuery node)
    {
        return new BoundNegatedTextQuery(true, node.Text);
    }

    private static BoundQuery NegateField(BoundNegatedFieldQuery node)
    {
        return new BoundNegatedFieldQuery(!node.IsNegated, node.Field, node.Value);
    }

    private static BoundQuery NegateFieldValue(BoundNegatedFieldValueQuery node)
    {
        return new BoundNegatedFieldValueQuery(!node.IsNegated, node.Value);
    }

    private static BoundQuery NegateText(BoundNegatedTextQuery node)
    {
        return new BoundNegatedTextQuery(!node.IsNegated, node.Text);
    }

    private static BoundQuery NegateNegatedQuery(BoundNegatedQuery node)
    {
        return node.Query;
    }

    private static BoundQuery NegateAnd(BoundAndQuery node)
    {
        return new BoundOrQuery(Negate(node.Left), Negate(node.Right));
    }

    private static BoundQuery NegateOr(BoundOrQuery node)
    {
        return new BoundAndQuery(Negate(node.Left), Negate(node.Right));
    }

    private static BoundDisjunction CreateDisjunction(BoundQuery node)
    {
        var stack = new Stack<BoundQuery>();
        stack.Push(node);

        var disjunctions = ImmutableArray.CreateBuilder<BoundQuery>();

        while (stack.Count > 0)
        {
            var n = stack.Pop();
            if (n is not BoundOrQuery or)
            {
                disjunctions.Add(CreateConjunction(n));
            }
            else
            {
                stack.Push(or.Right);
                stack.Push(or.Left);
            }
        }

        return new BoundDisjunction(disjunctions.ToImmutable());
    }

    private static BoundConjunction CreateConjunction(BoundQuery node)
    {
        var stack = new Stack<BoundQuery>();
        stack.Push(node);

        var conjunctions = ImmutableArray.CreateBuilder<BoundQuery>();

        while (stack.Count > 0)
        {
            var n = stack.Pop();
            if (n is not BoundAndQuery and)
            {
                conjunctions.Add(n);
            }
            else
            {
                stack.Push(and.Right);
                stack.Push(and.Left);
            }
        }

        return new BoundConjunction(conjunctions.ToImmutable());
    }
}
