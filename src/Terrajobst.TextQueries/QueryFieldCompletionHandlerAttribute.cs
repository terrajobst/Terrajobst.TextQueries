namespace Terrajobst.TextQueries;

[AttributeUsage(AttributeTargets.Method)]
public sealed class QueryFieldCompletionHandlerAttribute : Attribute
{
    public QueryFieldCompletionHandlerAttribute(params string[] pairs)
    {
    }
}
