namespace Terrajobst.TextQueries.Binding;

public sealed class BoundTextQuery : BoundQuery
{
    internal BoundTextQuery(string text)
    {
        ThrowIfNull(text);

        Text = text;
    }

    public new string Text { get; }
}
