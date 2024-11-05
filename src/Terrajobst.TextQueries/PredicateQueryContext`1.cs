using Terrajobst.TextQueries.Completion;

namespace Terrajobst.TextQueries;

public abstract class PredicateQueryContext<T> : PredicateQueryContext<T, object?>
{
    public Func<T, bool> Generate(string text)
    {
        return Generate(text, null);
    }

    public Func<T, bool> Generate(Query query)
    {
        return Generate(query, null);
    }

    protected override Func<T, bool> GenerateText(object? context, string text)
    {
        return GenerateText(text);
    }

    protected abstract Func<T, bool> GenerateText(string text);

    public QueryCompletionResult Complete(string text, int position)
    {
        return Complete(text, position, null);
    }

    public QueryCompletionResult Complete(Query query, int position)
    {
        return Complete(query, position, null);
    }
}
