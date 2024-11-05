namespace Terrajobst.TextQueries;

public abstract class PredicateQueryContext<TContext, T> : QueryContext<TContext, Func<T, bool>>
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

public abstract class PredicateQueryContext<T> : PredicateQueryContext<object?, T>
{
    public Func<T, bool> GenerateNode(Query query)
    {
        return GenerateNode(query, null);
    }

    protected override Func<T, bool> GenerateText(object context, string text)
    {
        return GenerateText(text);
    }

    protected abstract Func<T, bool> GenerateText(string text);
}
