using System.Linq.Expressions;

namespace Terrajobst.TextQueries;

public abstract class ExpressionQueryContext<T, TContext> : NodeQueryContext<TContext, Expression<Func<T, bool>>>
{
    protected override Expression<Func<T, bool>> GenerateNegated(TContext context, Expression<Func<T, bool>> argument)
    {
        var parameter = argument.Parameters.Single();
        var body = Expression.Not(argument.Body);
        return Expression.Lambda<Func<T, bool>>(body, parameter);
    }

    protected override Expression<Func<T, bool>> GenerateAnd(TContext context, Expression<Func<T, bool>> left, Expression<Func<T, bool>> right)
    {
        var parameter = left.Parameters.Single();
        var leftBody = left.Body;
        var rightBody = ReplaceParameter(right.Body, parameter);
        var body = Expression.And(leftBody, rightBody);
        return Expression.Lambda<Func<T, bool>>(body, parameter);
    }

    protected override Expression<Func<T, bool>> GenerateOr(TContext context, Expression<Func<T, bool>> left, Expression<Func<T, bool>> right)
    {
        var parameter = left.Parameters.Single();
        var leftBody = left.Body;
        var rightBody = ReplaceParameter(right.Body, parameter);
        var body = Expression.Or(leftBody, rightBody);
        return Expression.Lambda<Func<T, bool>>(body, parameter);
    }

    private static Expression ReplaceParameter(Expression expression, ParameterExpression parameter)
    {
        var replacer = new ParameterReplacer(parameter);
        return replacer.Visit(expression);
    }

    private sealed class ParameterReplacer : ExpressionVisitor
    {
        private readonly ParameterExpression _parameter;

        public ParameterReplacer(ParameterExpression parameter)
        {
            _parameter = parameter;
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            return _parameter;
        }
    }
}
