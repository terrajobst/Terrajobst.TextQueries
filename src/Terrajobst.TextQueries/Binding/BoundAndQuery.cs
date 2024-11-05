namespace Terrajobst.TextQueries.Binding;

internal sealed class BoundAndQuery : BoundQuery
{
    public BoundAndQuery(BoundQuery left, BoundQuery right)
    {
        ThrowIfNull(left);
        ThrowIfNull(right);

        Left = left;
        Right = right;
    }

    public BoundQuery Left { get; }
    public BoundQuery Right { get; }
}
