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

        private readonly Dictionary<string, Func<IScriptEvaluator, object[], object>> _functions = new Dictionary<string, Func<IScriptEvaluator, object[], object>>();

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
            Parser = new ExpressionParser(this);

            PrepareOperations();
        }

        public IParser Parser { get; set; }

        public void Register(string name, Delegate func)
        {
            var funcParameters = func.Method.GetParameters();

            var callerExpr = Expression.Parameter(typeof(IScriptEvaluator), "caller");
            var paraExpr = Expression.Parameter(typeof(object[]), "obj");
            var callExprs = new List<Expression>(funcParameters.Length);


            var converter = typeof(Convert).GetMethod("ChangeType", new[] { typeof(object), typeof(Type) });

            var objIndex = 0;
            foreach (var info in funcParameters)
            {
                var pType = info.ParameterType;
                if (pType.IsValueType || pType == typeof(string))
                {
                    var convert = Expression.Call(converter, Expression.ArrayIndex(paraExpr, Expression.Constant(objIndex++)), Expression.Constant(pType));
                    callExprs.Add(Expression.Convert(convert, pType));
                }
                else if (pType == typeof(IScriptEvaluator))
                {
                    callExprs.Add(callerExpr);
                }
                else if (Array.IndexOf(funcParameters, info) == funcParameters.Length - 1 &&
                    pType.BaseType == typeof(Array) &&
                    pType.GetElementType() is Type elementType && (elementType.IsValueType || elementType == typeof(string)))
                {
                    Func<object[], int, Type, IEnumerable<object>> arrayConverter = (obj, start, type) => obj.Skip(start).Select(o => Convert.ChangeType(o, type));
                    var ofTypeMethod = typeof(Enumerable).GetMethod("OfType").MakeGenericMethod(elementType);
                    var toArrayMethod= typeof(Enumerable).GetMethod("ToArray").MakeGenericMethod(elementType);

                    // objs.Skip(objIndex).Select(0=>Covert.ChangeType(o, type)).OfType<elementType>().ToArray()
                    var convertCallExpr = Expression.Call(toArrayMethod,
                        Expression.Call(ofTypeMethod,
                            Expression.Call(Expression.Constant(arrayConverter.Target), arrayConverter.Method,
                                paraExpr,
                                Expression.Constant(objIndex),
                                Expression.Constant(elementType))));

                    callExprs.Add(convertCallExpr);
                }
                else
                {
                    throw new ArgumentException("argument type must be value type or string", info.Name);
                }
            }

            var body = Expression.Call(func.Method.IsStatic ? null : Expression.Constant(func.Target), func.Method, callExprs);

            var isAction = func.Method.ReturnType == typeof(void);

            Func<IScriptEvaluator, object[], object> fun;
            if(isAction)
            {
                var _actionCaller = Expression.Lambda<Action<IScriptEvaluator, object[]>>(body, callerExpr, paraExpr).Compile();
                fun = (caller, obj) =>
                {
                    _actionCaller(caller, obj);
                    return null;
                };
            }
            else
            {
                var boxed = Expression.Convert(body, typeof(object));
                var _funcCaller = Expression.Lambda<Func<IScriptEvaluator, object[], object>>(boxed, callerExpr, paraExpr).Compile();
                fun = (caller, obj) =>
                {
                    return _funcCaller(caller, obj);
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

            var result = _functions[name](caller, arguments);

            caller.SetAlias("ret", result);

            if (name != "push" && name != "peek" && name != "pop" && name != "goto" && name != "goif")
            {
                EngineOperations.Push(caller, "eax");
            }

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
