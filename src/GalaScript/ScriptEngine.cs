using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using GalaScript.Internal;
using GalaScript.Interfaces;

namespace GalaScript
{
    public class ScriptEngine : IEngine
    {
        private IParser _parser;

        private readonly IEnumerable<string> _keywords = new string[0];

        private readonly Dictionary<string, Func<object[], object>> _functions = new Dictionary<string, Func<object[], object>>();

        private readonly Dictionary<string, Func<object[], object>> _operations = new Dictionary<string, Func<object[], object>>();

        private IScriptEvaluator _script;

        private IDropOutStack<object> _eax = new DropOutStack<object>(10);
        private IDropOutStack<object> _ebx = new DropOutStack<object>(10);
        private Dictionary<string, object> _aliases = new Dictionary<string, object>();

        private void PrepareOperations()
        {
            foreach (var method in typeof(EngineOperations).GetTypeInfo()
                .GetMethods(BindingFlags.Static | BindingFlags.Public))
            {
                _operations.Add(method.Name.ToLower(), objects =>
                {
                    try
                    {
                        return method.Invoke(method, objects);
                    }
                    catch (TargetInvocationException ex)
                    {
                        if (ex.InnerException == null) throw;

                        throw ex.InnerException;
                    }
                });
            }
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

        public IEnumerable<string> GetKeywords()
        {
            return _keywords;
        }

        public void Register(string name, Func<object[], object> func)
        {
            _functions[name] = func;
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

            var result = _operations.ContainsKey(name) ? _operations[name](parameters) : _functions[name](parameters);

            caller.SetAlias("ret", result);

            if (!_operations.ContainsKey(name))
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
