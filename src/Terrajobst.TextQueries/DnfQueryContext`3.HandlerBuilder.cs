using System.Reflection;

namespace Terrajobst.TextQueries;

public abstract partial class DnfQueryContext<TContext, TDisjunction, TConjunction>
{
    private sealed class HandlerBuilder : QueryProcessorBuilder<Action<TContext, TConjunction, bool, string>,
                                                                Func<TContext, IEnumerable<string>>>
    {
        public static HandlerBuilder Default { get; } = new();

        protected override Action<TContext, TConjunction, bool, string> GetFieldHandler(MethodInfo method)
        {
            var parameters = method.GetParameters();

            // Allowed signatures:
            //
            // void M(TConjunction conjunction, bool isNegated);
            // void M(TConjunction conjunction, bool isNegated, string value);
            // void M(TContext context, TConjunction conjunction, bool isNegated);
            // void M(TContext context, TConjunction conjunction, bool isNegated, string value);

            if (method.ReturnType != typeof(void))
                throw new Exception($"Wrong return type for {method}. Expected: void");

            if (parameters.Length == 2 && parameters[0].ParameterType == typeof(TConjunction) &&
                                          parameters[1].ParameterType == typeof(bool))
            {
                return (context, conjunction, isNegated, value) => method.Invoke(null, [conjunction, isNegated]);
            }
            else if (parameters.Length == 3 && parameters[0].ParameterType == typeof(TConjunction) &&
                                               parameters[1].ParameterType == typeof(bool) &&
                                               parameters[2].ParameterType == typeof(string))
            {
                return (context, conjunction, isNegated, value) => method.Invoke(null, [conjunction, isNegated, value]);
            }
            else if (parameters.Length == 3 && parameters[2].ParameterType == typeof(TContext) &&
                                               parameters[0].ParameterType == typeof(TConjunction) &&
                                               parameters[1].ParameterType == typeof(bool))
            {
                return (context, conjunction, isNegated, value) => method.Invoke(null, [context, conjunction, isNegated]);
            }
            else if (parameters.Length == 4 && parameters[0].ParameterType == typeof(TContext) &&
                                               parameters[1].ParameterType == typeof(TConjunction) &&
                                               parameters[2].ParameterType == typeof(bool) &&
                                               parameters[3].ParameterType == typeof(string))
            {
                return (context, conjunction, isNegated, value) => method.Invoke(null, [context, conjunction, isNegated, value]);
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
                return context => (IEnumerable<string>) method.Invoke(null, [])!;
            }
            else if (parameters.Length == 1 && parameters[0].ParameterType == typeof(TContext))
            {
                return context => (IEnumerable<string>) method.Invoke(null, [context])!;
            }
            else
            {
                throw new Exception($"Unexpected signature for {method}");
            }
        }
    }
}
