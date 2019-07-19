using System.Linq;
using System;
using System.Linq.Expressions;
using System.Reflection;

namespace GalaScript.Abstract
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

        #region [Action Extensions]

        public static void Register(this IEngine engine, string name, Action action)
            => engine.Register(name, action);

        public static void Register<T1>(this IEngine engine, string name, Action<T1> action)
            => engine.Register(name, action);

        public static void Register<T1, T2>(this IEngine engine, string name, Action<T1, T2> action)
            => engine.Register(name, action);

        public static void Register<T1, T2, T3>(this IEngine engine, string name, Action<T1, T2, T3> action)
            => engine.Register(name, action);

        public static void Register<T1, T2, T3, T4>(this IEngine engine, string name, Action<T1, T2, T3, T4> action)
            => engine.Register(name, action);

        public static void Register<T1, T2, T3, T4, T5>(this IEngine engine, string name, Action<T1, T2, T3, T4, T5> action)
            => engine.Register(name, action);

        public static void Register<T1, T2, T3, T4, T5, T6>(this IEngine engine, string name, Action<T1, T2, T3, T4, T5, T6> action)
            => engine.Register(name, action);

        public static void Register<T1, T2, T3, T4, T5, T6, T7>(this IEngine engine, string name, Action<T1, T2, T3, T4, T5, T6, T7> action)
            => engine.Register(name, action);

        public static void Register<T1, T2, T3, T4, T5, T6, T7, T8>(this IEngine engine, string name, Action<T1, T2, T3, T4, T5, T6, T7, T8> action)
            => engine.Register(name, action);

        public static void Register<T1, T2, T3, T4, T5, T6, T7, T8, T9>(this IEngine engine, string name, Action<T1, T2, T3, T4, T5, T6, T7, T8, T9> action)
            => engine.Register(name, action);

        public static void Register<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(this IEngine engine, string name, Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> action)
            => engine.Register(name, action);

        public static void Register<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(this IEngine engine, string name, Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> action)
            => engine.Register(name, action);

        public static void Register<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(this IEngine engine, string name, Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> action)
            => engine.Register(name, action);

        public static void Register<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(this IEngine engine, string name, Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> action)
            => engine.Register(name, action);

        public static void Register<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(this IEngine engine, string name, Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> action)
            => engine.Register(name, action);

        public static void Register<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(this IEngine engine, string name, Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> action)
            => engine.Register(name, action);

        public static void Register<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(this IEngine engine, string name, Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> action)
            => engine.Register(name, action);

        #endregion

        #region [Func Extensions]

        public static void Register<TOut>(this IEngine engine, string name, Func<TOut> func)
            => engine.Register(name, func);

        public static void Register<T1, TOut>(this IEngine engine, string name, Func<T1, TOut> func)
            => engine.Register(name, func);

        public static void Register<T1, T2, TOut>(this IEngine engine, string name, Func<T1, T2, TOut> func)
            => engine.Register(name, func);

        public static void Register<T1, T2, T3, TOut>(this IEngine engine, string name, Func<T1, T2, T3, TOut> func)
            => engine.Register(name, func);

        public static void Register<T1, T2, T3, T4, TOut>(this IEngine engine, string name, Func<T1, T2, T3, T4, TOut> func)
            => engine.Register(name, func);

        public static void Register<T1, T2, T3, T4, T5, TOut>(this IEngine engine, string name, Func<T1, T2, T3, T4, T5, TOut> func)
            => engine.Register(name, func);

        public static void Register<T1, T2, T3, T4, T5, T6, TOut>(this IEngine engine, string name, Func<T1, T2, T3, T4, T5, T6, TOut> func)
            => engine.Register(name, func);

        public static void Register<T1, T2, T3, T4, T5, T6, T7, TOut>(this IEngine engine, string name, Func<T1, T2, T3, T4, T5, T6, T7, TOut> func)
            => engine.Register(name, func);

        public static void Register<T1, T2, T3, T4, T5, T6, T7, T8, TOut>(this IEngine engine, string name, Func<T1, T2, T3, T4, T5, T6, T7, T8, TOut> func)
            => engine.Register(name, func);

        public static void Register<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOut>(this IEngine engine, string name, Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOut> func)
            => engine.Register(name, func);

        public static void Register<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOut>(this IEngine engine, string name, Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOut> func)
            => engine.Register(name, func);

        public static void Register<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOut>(this IEngine engine, string name, Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOut> func)
            => engine.Register(name, func);

        public static void Register<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOut>(this IEngine engine, string name, Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOut> func)
            => engine.Register(name, func);

        public static void Register<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOut>(this IEngine engine, string name, Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOut> func)
            => engine.Register(name, func);

        public static void Register<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOut>(this IEngine engine, string name, Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOut> func)
            => engine.Register(name, func);

        public static void Register<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TOut>(this IEngine engine, string name, Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TOut> func)
            => engine.Register(name, func);

        public static void Register<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TOut>(this IEngine engine, string name, Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TOut> func)
            => engine.Register(name, func);

        #endregion
    }
}
