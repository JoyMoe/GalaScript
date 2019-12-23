using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading;
using GalaScript.Exceptions;
using GalaScript.Interfaces;
using GalaScript.Internal;

namespace GalaScript
{
    public class ScriptEngine : IScriptEngine
    {
        private bool _paused;

        private readonly Dictionary<string, Func<IScriptEngine, IScriptEvaluator, object[], object>> _functions = new Dictionary<string, Func<IScriptEngine, IScriptEvaluator, object[], object>>();

        private IScriptEvaluator _script;

        private Stack<object> _stack = new Stack<object>();

        private Dictionary<string, object> _aliases = new Dictionary<string, object>();

        private static readonly object Void = new object();

        public bool IsCancellationRequested { get; private set; }

        public bool IsDebugAllowed { get; }

        public CancellationTokenSource PauseTokenSource { get; private set; }

        public bool Paused
        {
            get => _paused;
            internal set
            {
                if (_paused == value)
                {
                    return;
                }

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

        public IScriptEvaluator Current { get; internal set; }

        private void PrepareOperations()
        {
            foreach (var cls in typeof(InternalOperations).GetNestedTypes())
            {
                foreach (var method in cls.GetMethods(BindingFlags.Static | BindingFlags.Public))
                {
                    this.Register(method.Name.ToLower(), method);
                }
            }
        }

        public ScriptEngine(bool debug = false)
        {
            IsDebugAllowed = debug;

            Parser = new ExpressionParser(this);

            PrepareOperations();
        }

        public void Register(string name, Delegate func)
        {
            var funcParameters = func.Method.GetParameters();

            var engineExpr = Expression.Parameter(typeof(IScriptEngine), "engine");
            var callerExpr = Expression.Parameter(typeof(IScriptEvaluator), "caller");
            var paraExpr = Expression.Parameter(typeof(object[]), "obj");
            var optionsExpr = Expression.Parameter(typeof(Dictionary<string, object>), "options");
            var callExpr = new List<Expression>(funcParameters.Length);

            // paraLen = obj.Length
            var paraLenExpr = Expression.Property(paraExpr, "Length");

            var converter = typeof(Convert).GetMethod("ChangeType", new[] {typeof(object), typeof(Type)});

            static object GetValueOrThrow(Dictionary<string, object> dict, string key)
            {
                if (dict.ContainsKey(key))
                    return dict[key];
                throw new ArgumentException($"Missing argument: {key}");
            }

            Func<Dictionary<string, object>, string, object> GetValueOrThrowFunc = GetValueOrThrow;

            int objIndex = 0;
            foreach (var info in funcParameters)
            {
                var pType = info.ParameterType;
                if (pType.IsValueType || pType == typeof(string) || pType == typeof(object))
                {
                    // value = i < paraLen ? obj[i] : ( options[key] ?? throw )
                    Expression valueExpr = Expression.Condition(
                        Expression.LessThan(Expression.Constant(objIndex), paraLenExpr),
                        Expression.Convert(Expression.Call(converter, Expression.ArrayIndex(paraExpr, Expression.Constant(objIndex)), Expression.Constant(pType)), pType),
                        Expression.Convert(Expression.Call(converter, Expression.Call(GetValueOrThrowFunc.Method, optionsExpr, Expression.Constant(info.Name)), Expression.Constant(pType)), pType)
                    );
                    callExpr.Add(Expression.Convert(valueExpr, pType));

                    objIndex++;
                }
                else if (pType == typeof(IScriptEngine))
                {
                    callExpr.Add(engineExpr);
                }
                else if (pType == typeof(IScriptEvaluator))
                {
                    callExpr.Add(callerExpr);
                }
                else if (Array.IndexOf(funcParameters, info) == funcParameters.Length - 1 &&
                         pType.BaseType == typeof(Array) && pType.GetElementType() is Type elementType)
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
                    else if (elementType == typeof(object))
                    {
                        callExpr.Add(Expression.Call(toArrayMethod, Expression.Call(skipMethod, paraExpr, Expression.Constant(objIndex))));
                    }
                }
                else
                {
                    throw new ArgumentException("Argument type must be value type or string", info.Name);
                }
            }

            var body = Expression.Call(func.Method.IsStatic ? null : Expression.Constant(func.Target), func.Method, callExpr);

            bool isAction = func.Method.ReturnType == typeof(void);

            var defaultDict = funcParameters.Where(p => p.HasDefaultValue).ToDictionary(p => p.Name, p => p.DefaultValue);

            void SplitArgs(object[] args, out object[] values, out Dictionary<string, object> dict)
            {
                var kvs = args.OfType<KeyValuePair<string, object>>().ToArray();
                if (kvs.Length == 0)
                {
                    dict = defaultDict;
                    values = args;
                }

                dict = new Dictionary<string, object>(defaultDict);
                foreach (var kv in kvs)
                    dict[kv.Key] = kv.Value;
                values = args.Take(args.Length - kvs.Length).ToArray();
            }

            Func<IScriptEngine, IScriptEvaluator, object[], object> fun;
            if (isAction)
            {
                var actionCaller = Expression.Lambda<Action<IScriptEngine, IScriptEvaluator, object[], Dictionary<string, object>>>(body, engineExpr, callerExpr, paraExpr, optionsExpr).Compile();
                fun = (engine, caller, args) =>
                {
                    SplitArgs(args, out var values, out var dict);
                    actionCaller(engine, caller, values, dict);
                    return Void;
                };
            }
            else
            {
                var boxed = Expression.Convert(body, typeof(object));

                var funcCaller = Expression.Lambda<Func<IScriptEngine, IScriptEvaluator, object[], Dictionary<string, object>, object>>(boxed, engineExpr, callerExpr, paraExpr, optionsExpr).Compile();
                fun = (engine, caller, args) =>
                {
                    SplitArgs(args, out var values, out var dict);
                    return funcCaller(engine, caller, values, dict);
                };
            }

            _functions[name] = fun;
        }

        public object Call(IScriptEvaluator caller, string name, params object[] arguments)
        {
            if (caller == null)
            {
                caller = _script;
            }

            if (!_functions.ContainsKey(name))
            {
                throw new MethodNotFoundException(name);
            }

            var result = _functions[name](this, caller, arguments);

            if (result == Void)
            {
                return result;
            }

            caller?.SetAlias("ret", result);

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

            Current = _script;
        }

        public void Reset()
        {
            _stack.Clear();
            _aliases.Clear();

            Current = null;
            IsCancellationRequested = false;
            PauseTokenSource = null;

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

        public object StepIn()
        {
            return Current?.StepIn();
        }

        public void Cancel()
        {
            IsCancellationRequested = true;
        }

        public void Continue()
        {
            if (Paused) PauseTokenSource.Cancel();
        }

        public void Pause()
        {
            PauseTokenSource = new CancellationTokenSource();
        }
    }
}
