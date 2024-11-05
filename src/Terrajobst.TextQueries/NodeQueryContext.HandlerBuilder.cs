using System.Reflection;

namespace Terrajobst.TextQueries;

public abstract partial class NodeQueryContext<TContext, TNode>
{
    private sealed class HandlerBuilder : QueryProcessorBuilder<Func<TContext, string, TNode>, Func<TContext, IEnumerable<string>>>
    {
        public static HandlerBuilder Default { get; } = new();

        protected override Func<TContext, string, TNode> GetFieldHandler(MethodInfo method)
        {
            var parameters = method.GetParameters();

            // Allowed signatures:
            //
            // TNode M()
            // TNode M(string value)
            // TNode M(TContext context)
            // TNode M(TContext context, string value)

            if (method.ReturnType != typeof(TNode))
                throw new Exception($"Wrong return type for {method}. Expected: {typeof(TNode).Name}");

            if (parameters.Length == 0)
            {
                return (context, value) => (TNode)method.Invoke(null, null)!;
            }
            else if (parameters.Length == 1 && parameters[0].ParameterType == typeof(string))
            {
                return (context, value) => (TNode)method.Invoke(null, [value])!;
            }
            else if (parameters.Length == 1 && parameters[0].ParameterType == typeof(TContext))
            {
                return (context, value) => (TNode)method.Invoke(null, [context])!;
            }
            else if (parameters.Length == 2 && parameters[0].ParameterType == typeof(TContext) &&
                                               parameters[1].ParameterType == typeof(string))
            {
                return (context, value) => (TNode)method.Invoke(null, [context, value])!;
            }
            else
            {
                throw new Exception($"Unexpected signature for {method}");
            }
        }

        protected override Func<TContext, IEnumerable<string>> GetCompletionHandler(MethodInfo method)
        {
            var parameters = method.GetParameters();

            // Allowed signatures:
            //
            // IEnumerable<string> M();
            // IEnumerable<string> void M(TContext context);

            if (method.ReturnType != typeof(IEnumerable<string>))
                throw new Exception($"Wrong return type for {method}. Expected: IEnumerable<string>");

            if (parameters.Length == 0)
            {
                return context => (IEnumerable<string>)method.Invoke(null, [])!;
            }
            else if (parameters.Length == 1 && parameters[0].ParameterType == typeof(TContext))
            {
                return context => (IEnumerable<string>)method.Invoke(null, [context])!;
            }
            else
            {
                throw new Exception($"Unexpected signature for {method}");
            }
        }
    }
}
