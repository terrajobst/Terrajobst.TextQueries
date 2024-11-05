namespace Terrajobst.TextQueries.Binding;

public sealed class BoundAndQuery : BoundQuery
{
    internal BoundAndQuery(BoundQuery left, BoundQuery right)
    {
        ThrowIfNull(left);
        ThrowIfNull(right);

        Left = left;
        Right = right;
    }

    public BoundQuery Left { get; }
    public BoundQuery Right { get; }
}
