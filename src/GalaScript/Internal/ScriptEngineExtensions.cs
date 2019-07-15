using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using GalaScript.Interfaces;

namespace GalaScript.Internal
{
    public static class ScriptEngineExtensions
    {
        public static void Register(this IEngine engine, string name, MethodInfo methodInfo)
        {
            var args = methodInfo.GetParameters().Select(p => p.ParameterType);
            var delegateType = methodInfo.ReturnType == typeof(void) ?
                Expression.GetActionType(args.ToArray()) :
                Expression.GetFuncType(args.Concat(new[] { methodInfo.ReturnType }).ToArray());
            engine.Register(name, methodInfo.CreateDelegate(delegateType));
        }
    }
}
