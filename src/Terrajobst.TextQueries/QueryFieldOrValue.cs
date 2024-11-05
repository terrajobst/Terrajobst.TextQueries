namespace Terrajobst.TextQueries;

public abstract class QueryFieldOrValue
{
    private protected QueryFieldOrValue(string name, string description)
    {
        ThrowIfNullOrEmpty(name);
        ThrowIfNull(name);

        Name = name;
        Description = description;
    }

    public string Name { get; }

    public string Description { get; }
}
