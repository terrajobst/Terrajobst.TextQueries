namespace Terrajobst.TextQueries.Binding;

internal sealed class BoundTextQuery : BoundQuery
{
    public BoundTextQuery(string text)
    {
        ThrowIfNull(text);

        Text = text;
    }

    public new string Text { get; }
}
