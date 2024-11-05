using System.Linq.Expressions;

using Terrajobst.TextQueries.Completion;

namespace Terrajobst.TextQueries;

public abstract class ExpressionQueryContext<T> : ExpressionQueryContext<T, object?>
{
    public Expression<Func<T, bool>> Generate(string text)
    {
        return Generate(text, null);
    }

    public Expression<Func<T, bool>> Generate(Query query)
    {
        return Generate(query, null);
    }

    protected override Expression<Func<T, bool>> GenerateText(object? context, string text)
    {
        return GenerateText(text);
    }

    protected abstract Expression<Func<T, bool>> GenerateText(string text);

    public QueryCompletionResult Complete(string text, int position)
    {
        return Complete(text, position, null);
    }

    public QueryCompletionResult Complete(Query query, int position)
    {
        return Complete(query, position, null);
    }
}
