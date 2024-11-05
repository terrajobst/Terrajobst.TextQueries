namespace Terrajobst.TextQueries.Syntax;

public readonly struct TextSpan
{
    public TextSpan(int start, int length)
    {
        ThrowIfNegative(start);
        ThrowIfNegative(length);

        Start = start;
        Length = length;
    }

    public static TextSpan FromBounds(int start, int end)
    {
        ThrowIfNegative(start);
        ThrowIfLessThan(end, start);

        var length = end - start;
        return new TextSpan(start, length);
    }

    public int Start { get; }
    public int End => Start + Length;
    public int Length { get; }

    public override string ToString() => $"[{Start},{End})";
}
