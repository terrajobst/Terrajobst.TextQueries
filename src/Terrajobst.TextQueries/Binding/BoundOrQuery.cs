namespace Terrajobst.TextQueries.Binding;

internal sealed class BoundOrQuery : BoundQuery
{
    public BoundOrQuery(BoundQuery left, BoundQuery right)
    {
        ThrowIfNull(left);
        ThrowIfNull(right);

        Left = left;
        Right = right;
    }

    public BoundQuery Left { get; }
    public BoundQuery Right { get; }
}
