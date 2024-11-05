namespace Terrajobst.TextQueries.Binding;

internal sealed class BoundNegatedTextQuery : BoundQuery
{
    public BoundNegatedTextQuery(bool isNegated, string text)
    {
        ThrowIfNull(text);

        IsNegated = isNegated;
        Text = text;
    }

    public bool IsNegated { get; }

    public new string Text { get; }
}
