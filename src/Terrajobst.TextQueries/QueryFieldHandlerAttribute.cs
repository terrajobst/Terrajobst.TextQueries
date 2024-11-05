namespace Terrajobst.TextQueries;

[AttributeUsage(AttributeTargets.Method)]
public sealed class QueryFieldHandlerAttribute : Attribute
{
    public QueryFieldHandlerAttribute(params string[] pairs)
    {
    }

    public string Description { get; set; } = "";
}
