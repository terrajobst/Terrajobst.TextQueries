using System.CodeDom.Compiler;

namespace Terrajobst.TextQueries.Binding;

internal abstract class BoundQuery
{
    private protected BoundQuery()
    {
    }

    public override string ToString()
    {
        using var stringWriter = new StringWriter();
        {
            using var indentedTextWriter = new IndentedTextWriter(stringWriter);
            Walk(indentedTextWriter, this);

            return stringWriter.ToString();
        }

        static void Walk(IndentedTextWriter writer, BoundQuery query)
        {
            switch (query)
            {
                case BoundFieldQuery q:
                    writer.WriteLine($"{q.Field}:{q.Value}");
                    break;
                case BoundFieldValueQuery q:
                    writer.WriteLine($"{q.Field}:{q.Value.Value}");
                    break;
                case BoundTextQuery q:
                    writer.WriteLine($"{q.Text}");
                    break;
                case BoundNegatedQuery q:
                    writer.WriteLine("NOT");
                    writer.Indent++;
                    Walk(writer, q.Query);
                    writer.Indent--;
                    break;
                case BoundAndQuery q:
                    writer.WriteLine("AND");
                    writer.Indent++;
                    Walk(writer, q.Left);
                    Walk(writer, q.Right);
                    writer.Indent--;
                    break;
                case BoundOrQuery q:
                    writer.WriteLine("OR");
                    writer.Indent++;
                    Walk(writer, q.Left);
                    Walk(writer, q.Right);
                    writer.Indent--;
                    break;
                default:
                    throw new Exception($"Unexpected query {query.GetType()}");
            }
        }
    }

    public static BoundFieldQuery Field(QueryField field, string value)
    {
        ThrowIfNull(field);
        ThrowIfNull(value);

        return new BoundFieldQuery(field, value);
    }

    public static BoundFieldValueQuery FieldValue(QueryFieldValue value)
    {
        ThrowIfNull(value);

        return new BoundFieldValueQuery(value);
    }

    public static BoundTextQuery Text(string text)
    {
        ThrowIfNull(text);

        return new BoundTextQuery(text);
    }

    public static BoundNegatedQuery Negate(BoundQuery argument)
    {
        ThrowIfNull(argument);

        return new BoundNegatedQuery(argument);
    }

    public static BoundAndQuery And(BoundQuery left, BoundQuery right)
    {
        ThrowIfNull(left);
        ThrowIfNull(right);

        return new BoundAndQuery(left, right);
    }

    public static BoundOrQuery Or(BoundQuery left, BoundQuery right)
    {
        ThrowIfNull(left);
        ThrowIfNull(right);

        return new BoundOrQuery(left, right);
    }
}
