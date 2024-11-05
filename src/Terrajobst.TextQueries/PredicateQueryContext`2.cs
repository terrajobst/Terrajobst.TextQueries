namespace Terrajobst.TextQueries;

public abstract class PredicateQueryContext<T, TContext> : NodeQueryContext<TContext, Func<T, bool>>
{
    protected override Func<T, bool> GenerateNegated(TContext context, Func<T, bool> argument)
    {
        return t => !argument(t);
    }

    protected override Func<T, bool> GenerateAnd(TContext context, Func<T, bool> left, Func<T, bool> right)
    {
        return t => left(t) && right(t);
    }

    protected override Func<T, bool> GenerateOr(TContext context, Func<T, bool> left, Func<T, bool> right)
    {
        return t => left(t) || right(t);
    }
}
