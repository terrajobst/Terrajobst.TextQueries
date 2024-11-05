using System.Text;

using Terrajobst.TextQueries.Binding;
using Terrajobst.TextQueries.Completion;

using static System.Net.Mime.MediaTypeNames;

namespace Terrajobst.TextQueries;

public abstract class QueryContext<TContext, TResult> : QueryContext
{
    private protected QueryContext()
    {
    }

    public TResult Generate(string text, TContext context)
    {
        ThrowIfNull(text);

        var query = CreateQuery(text);
        if (query.Diagnostics.Any(d => d.IsError))
        {
            var sb = new StringBuilder();
            sb.AppendLine("The query has errors:");
            sb.AppendLine();

            foreach (var d in query.Diagnostics)
            {
                if (d.IsError)
                    sb.Append("[ERROR] : ");
                else
                    sb.Append("[WARN]  : ");

                sb.Append(d.Message);
                sb.AppendLine();
            }

            throw new Exception(sb.ToString());
        }

        return GenerateQuery(context, query.BoundQuery);
    }

    public TResult Generate(Query query, TContext context)
    {
        ThrowIfNull(query);

        if (!ReferenceEquals(query.Context, this))
            throw new ArgumentException("The query belongs to a different QueryContext", nameof(query));

        return GenerateQuery(context, query.BoundQuery);
    }

    public QueryCompletionResult Complete(string text, int position, TContext context)
    {
        ThrowIfNull(text);

        var query = CreateQuery(text);
        return Complete(query, position, context);
    }

    public QueryCompletionResult Complete(Query query, int position, TContext context)
    {
        ThrowIfNull(query);

        var provider = CreateCompletionProvider(context);
        return provider.Complete(query.Syntax, position);
    }

    private protected abstract TResult GenerateQuery(TContext context, BoundQuery query);

    private protected abstract QueryCompletionProvider CreateCompletionProvider(TContext context);
}
