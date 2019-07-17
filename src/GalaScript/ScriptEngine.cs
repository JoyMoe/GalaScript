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
        private readonly Dictionary<string, Func<IEngine, IScriptEvaluator, object[], object>> _functions = new Dictionary<string, Func<IEngine, IScriptEvaluator, object[], object>>();

        private bool _paused;

        private IScriptEvaluator _script;

        private Stack<object> _stack = new Stack<object>();

        private Dictionary<string, object> _aliases = new Dictionary<string, object>();

        private static readonly object Void = new object();

        private void PrepareOperations()
        {
            foreach (var method in typeof(EngineOperations).GetMethods(BindingFlags.Static | BindingFlags.Public))
                this.Register(method.Name.ToLower(), method);
        }

        public ScriptEngine(bool debug = false)
        {
            Debug = debug;

            Parser = new ExpressionParser(this);

            PrepareOperations();
        }

        public bool Debug { get; }

        public bool Paused
        {
            get => _paused;
            set
            {
                _paused = value;

                if (_paused)
                {
                    OnPausedHandler?.Invoke();
                }
                else
                {
                    OnResumedHandler?.Invoke();
                }
            }
        }

        public event EngineEventHandler OnStartedHandler;

        public event EngineEventHandler OnPausedHandler;

        public event EngineEventHandler OnResumedHandler;

        public event EngineEventHandler OnExitedHandler;

        public IParser Parser { get; set; }

        public IScriptEvaluator Current { get; set; }

        public void Register(string name, Delegate func)
        {
            var funcParameters = func.Method.GetParameters();

            var engineExpr = Expression.Parameter(typeof(IEngine), "engine");
            var callerExpr = Expression.Parameter(typeof(IScriptEvaluator), "caller");
            var paraExpr = Expression.Parameter(typeof(object[]), "obj");
            var callExpr = new List<Expression>(funcParameters.Length);

            var converter = typeof(Convert).GetMethod("ChangeType", new[] { typeof(object), typeof(Type) });

            var objIndex = 0;
            foreach (var info in funcParameters)
            {
                var pType = info.ParameterType;
                if (pType.IsValueType || pType == typeof(string))
                {
                    // ReSharper disable AssignNullToNotNullAttribute
                    var convert = Expression.Call(converter, Expression.ArrayIndex(paraExpr, Expression.Constant(objIndex++)), Expression.Constant(pType));
                    // ReSharper restore AssignNullToNotNullAttribute
                    callExpr.Add(Expression.Convert(convert, pType));
                }
                else if (pType == typeof(IEngine))
                {
                    callExpr.Add(engineExpr);
                }
                else if (pType == typeof(IScriptEvaluator))
                {
                    callExpr.Add(callerExpr);
                }
                else if (Array.IndexOf(funcParameters, info) == funcParameters.Length - 1 && pType.BaseType == typeof(Array) && pType.GetElementType() is Type elementType)
                {
                    // ReSharper disable PossibleNullReferenceException
                    var ofTypeMethod = typeof(Enumerable).GetMethod("OfType").MakeGenericMethod(elementType);
                    var toArrayMethod = typeof(Enumerable).GetMethod("ToArray").MakeGenericMethod(elementType);
                    var skipMethod = typeof(Enumerable).GetMethod("Skip").MakeGenericMethod(elementType);
                    // ReSharper restore PossibleNullReferenceException

                    if (elementType.IsValueType || elementType == typeof(string))
                    {
                        Func<object[], int, Type, IEnumerable<object>> arrayConverter = (obj, start, type) => obj.Skip(start).Select(o => Convert.ChangeType(o, type));

                        var convertCallExpr = Expression.Call(toArrayMethod,
                            Expression.Call(ofTypeMethod,
                                Expression.Call(Expression.Constant(arrayConverter.Target), arrayConverter.Method,
                                    paraExpr,
                                    Expression.Constant(objIndex),
                                    Expression.Constant(elementType))));

                        callExpr.Add(convertCallExpr);
                    }
                    else if(elementType == typeof(object))
                    {
                        callExpr.Add(Expression.Call(toArrayMethod, Expression.Call(skipMethod, paraExpr, Expression.Constant(objIndex))));
                    }
                }
                else
                {
                    throw new ArgumentException("argument type must be value type or string", info.Name);
                }
            }

            var body = Expression.Call(func.Method.IsStatic ? null : Expression.Constant(func.Target), func.Method, callExpr);

            var isAction = func.Method.ReturnType == typeof(void);

            Func<IEngine, IScriptEvaluator, object[], object> fun;
            if(isAction)
            {
                var actionCaller = Expression.Lambda<Action<IEngine, IScriptEvaluator, object[]>>(body, engineExpr, callerExpr, paraExpr).Compile();
                fun = (engine, caller, obj) =>
                {
                    actionCaller(engine, caller, obj);
                    return Void;
                };
            }
            else
            {
                var boxed = Expression.Convert(body, typeof(object));
                var funcCaller = Expression.Lambda<Func<IEngine, IScriptEvaluator, object[], object>>(boxed, engineExpr,callerExpr, paraExpr).Compile();
                fun = (engine, caller, obj) => funcCaller(engine, caller, obj);
            }

            _functions[name] = fun;
        }

        public object Call(IScriptEvaluator caller, string name , params object[] arguments)
        {
            if (caller == null)
            {
                caller = _script;
            }

            var result = _functions[name](this, caller, arguments);

            if (result == Void)
            {
                return result;
            }

            caller.SetAlias("ret", result);

            return result;
        }

        public void SetAlias(IScriptEvaluator caller, string name, object value)
        {
            caller.SetAlias(name, value);
        }

        public object GetAlias(IScriptEvaluator caller, string name)
        {
            return caller.GetAlias(name);
        }

        public void Prepare(string str, Encoding encoding = null)
        {
            _script = File.Exists(str)
                ? Parser.LoadFile(str, encoding)
                : Parser.LoadString(str);

            _script.ReplaceEnvironment(ref _stack, ref _aliases);
        }

        public void Reset()
        {
            _stack.Clear();
            _aliases.Clear();

            Current = null;
            Paused = false;

            _script?.Reset();
        }

        public object Run()
        {
            OnStartedHandler?.Invoke();

            var result = _script?.Evaluate();

            OnExitedHandler?.Invoke();

            return result;
        }

        public object Run(string str, Encoding encoding = null)
        {
            Prepare(str, encoding);

            return Run();
        }
    }
}
