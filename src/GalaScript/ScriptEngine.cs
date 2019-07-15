using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
using System.Linq.Expressions;
using GalaScript.Internal;
using GalaScript.Interfaces;
using System.Reflection;

namespace GalaScript
{
    public class ScriptEngine : IEngine
    {
        private IParser _parser;

        private readonly Dictionary<string, Func<object[], object>> _functions = new Dictionary<string, Func<object[], object>>();

        private IScriptEvaluator _script;

        private IDropOutStack<object> _eax = new DropOutStack<object>(10);
        private IDropOutStack<object> _ebx = new DropOutStack<object>(10);
        private Dictionary<string, object> _aliases = new Dictionary<string, object>();

        private void PrepareOperations()
        {
            foreach (var method in typeof(EngineOperations).GetMethods(BindingFlags.Static | BindingFlags.Public))
                this.Register(method.Name.ToLower(), method);
        }

        public ScriptEngine()
        {
            SetParser(new ExpressionParser(this));

            PrepareOperations();
        }

        public void SetParser(IParser parser)
        {
            _parser = parser;
        }

        public IParser GetParser()
        {
            return _parser;
        }

        public void Register(string name, Delegate func)
        {
            var funcParameters = func.Method.GetParameters();
            var paraExpr = Expression.Parameter(typeof(object[]), "obj");
            var callExprs = new Expression[funcParameters.Length];

            var isCallerRequired = 1;
            var converter = typeof(Convert).GetMethod("ChangeType", new[] { typeof(object), typeof(Type) });

            for (var i = 0; i < funcParameters.Length; i++)
            {
                var info = funcParameters[i];
                //if (info.ParameterType == typeof(IEngine))
                //{
                //    callExprs[i] = Expression.Constant(this);
                //}
                if (info.ParameterType == typeof(IScriptEvaluator))
                {
                    callExprs[i] = Expression.Convert(Expression.ArrayIndex(paraExpr, Expression.Constant(0)), typeof(IScriptEvaluator));
                    isCallerRequired = 0;
                }
                else if (info.ParameterType.IsValueType || info.ParameterType == typeof(string))
                {
                    var convert = Expression.Call(converter, Expression.ArrayIndex(paraExpr, Expression.Constant(i + isCallerRequired)), Expression.Constant(info.ParameterType));
                    callExprs[i] = Expression.Convert(convert, info.ParameterType);
                }
                else
                {
                    throw new ArgumentException("argument type must be value type or string", info.Name);
                }
            }

            var body = Expression.Call(func.Method.IsStatic ? null : Expression.Constant(func.Target), func.Method, callExprs);

            var isAction = func.Method.ReturnType == typeof(void);

            Func<object[], object> fun;
            if(isAction)
            {
                var _actionCaller = Expression.Lambda<Action<object[]>>(body, paraExpr).Compile();
                fun = (obj) =>
                {
                    _actionCaller(obj);
                    return null;
                };
            }
            else
            {
                var boxed = Expression.Convert(body, typeof(object));
                var _funcCaller = Expression.Lambda<Func<object[], object>>(boxed, paraExpr).Compile();
                fun = (obj) =>
                {
                    return _funcCaller(obj);
                };
            }

            _functions[name] = fun;
        }

        public object Call(IScriptEvaluator caller, string name , params object[] arguments)
        {
            if (caller == null)
            {
                caller = _script;
            }

            var parameters = new object[arguments.Length + 1];
            Array.Copy(arguments, 0, parameters, 1, arguments.Length);
            parameters[0] = caller;

            var result = _functions[name](parameters);

            caller.SetAlias("ret", result);

            if (name != "push" && name != "peek" && name != "pop" && name != "goto" && name != "goif")
            {
                EngineOperations.Push(caller, "eax");
            }

            return result;
        }

        public void SetAlias(IScriptEvaluator caller, string name, object value)
        {
            if (caller == null)
            {
                caller = _script;
            }

            caller.SetAlias(name, value);
        }

        public object GetAlias(IScriptEvaluator caller, string name)
        {
            if (caller == null)
            {
                caller = _script;
            }

            return caller.GetAlias(name);
        }

        public void Prepare(string str, Encoding encoding = null)
        {
            _script = File.Exists(str)
                ? _parser.LoadFile(str, encoding)
                : _parser.LoadString(str);

            _script.ReplaceEnvironment(ref _eax, ref _ebx, ref _aliases);
        }

        public object Evaluate()
        {
            _script?.Reset();

            return _script?.Evaluate();
        }

        public object Evaluate(string str, Encoding encoding = null)
        {
            Prepare(str, encoding);

            return Evaluate();
        }
    }
}
