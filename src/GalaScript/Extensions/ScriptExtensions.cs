using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

// ReSharper disable once CheckNamespace
namespace GalaScript.Interfaces
{
    public static class ScriptEngineExtensions
    {
        public static void Register(this IScriptEngine engine, string name, MethodInfo methodInfo)
        {
            var args = methodInfo.GetParameters().Select(p => p.ParameterType);
            var delegateType = methodInfo.ReturnType == typeof(void)
                ? Expression.GetActionType(args.ToArray())
                : Expression.GetFuncType(args.Concat(new[] {methodInfo.ReturnType}).ToArray());
            engine.Register(name, methodInfo.CreateDelegate(delegateType));
        }
    }
}
