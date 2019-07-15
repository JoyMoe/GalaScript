using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GalaScript.Interfaces;
using GalaScript.Internal;

namespace GalaScript.Evaluators
{
    public class ScriptEvaluator : IScriptEvaluator
    {
        private LinkedListNode<IEvaluator> _current;
        private readonly LinkedList<IEvaluator> _script = new LinkedList<IEvaluator>();
        private readonly Dictionary<string, LinkedListNode<IEvaluator>> _labels = new Dictionary<string, LinkedListNode<IEvaluator>>();

        private IDropOutStack<object> _eax = new DropOutStack<object>(10);
        private IDropOutStack<object> _ebx = new DropOutStack<object>(10);
        private Dictionary<string, object> _aliases = new Dictionary<string, object>();

        public ScriptEvaluator(IEngine engine, string str, bool isRootScriptEvaluator = true)
        {
            var evaluators = engine.GetParser().Prepare(str);

            foreach (var exp in evaluators)
            {
                if (exp == null) continue;

                if (!isRootScriptEvaluator)
                {
                    exp.SetCaller(this);
                }

                switch (exp)
                {
                    case MacroEvaluator macro:
                        macro.ReplaceEnvironment(_aliases);

                        object Macro(object[] objects)
                        {
                            macro.SetCaller(objects.FirstOrDefault() as IScriptEvaluator);
                            macro.SetArguments(objects.Skip(1));
                            return macro.Evaluate();
                        }

                        engine.Register(macro.GetName(), (Func<object[], object>) Macro);

                        break;
                    case ScriptEvaluator sub:
                        // TODO: add option to config ReplaceEnvironment in sub-script
                        sub.ReplaceEnvironment(ref _eax, ref _ebx, ref _aliases);
                        break;
                }

                _script.AddLast(exp);

                if (exp is LabelEvaluator label)
                {
                    _labels[label.GetName()] = _script.Last;
                }
            }

            Reset();
        }

        public void SetCaller(IScriptEvaluator caller)
        {
            // TODO: Set caller for Script
        }

        public object Evaluate()
        {
            while (_current != null)
            {
                StepOut();
            }

            return GetReturn();
        }

        public void Goto(string label)
        {
            if (_labels.TryGetValue(label, out var node))
            {
                _current = node;
            }
            else
            {
                throw new ArgumentException(nameof(label));
            }
        }

        public void Seek(long offset, SeekOrigin origin)
        {
            switch (origin)
            {
                case SeekOrigin.Begin:
                    _current = _script.First;
                    break;
                case SeekOrigin.Current:
                    break;
                case SeekOrigin.End:
                    _current = _script.Last;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(origin), origin, null);
            }

            for (var i = 0; i < offset; i++)
            {
                _current = _current?.Next;
            }
        }

        public void Reset()
        {
            if (_labels.ContainsKey("start"))
            {
                Goto("start");
            }
            else
            {
                Seek(0, SeekOrigin.Begin);
            }
        }

        public object StepOut()
        {
            var current = _current.Value;

            if (current != null && current is MacroEvaluator == false)
            {
                _current.Value?.Evaluate();
            }

            Seek(1, SeekOrigin.Current);

            return GetReturn();
        }

        public object GetReturn()
        {
            return GetAlias("ret");
        }

        public void ReplaceEnvironment(Dictionary<string, object> aliases)
        {
            foreach (var alias in aliases)
            {
                _aliases[alias.Key] = alias.Value;
            }
        }

        public void ReplaceEnvironment(ref Dictionary<string, object> aliases)
        {
            _aliases = aliases;
        }

        public void ReplaceEnvironment(ref IDropOutStack<object> eax, ref IDropOutStack<object> ebx,
            ref Dictionary<string, object> aliases)
        {
            _eax = eax;
            _ebx = ebx;
            _aliases = aliases;
        }

        public void SetAlias(string name, object value)
        {
            _aliases[name] = value;
        }

        public object GetAlias(string name)
        {
            return _aliases.ContainsKey(name) ? _aliases[name] : null;
        }

        public void Push(string reg)
        {
            switch (reg)
            {
                case "eax":
                    _eax.Push(GetReturn());
                    break;
                case "ebx":
                    _ebx.Push(GetReturn());
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(reg));
            }
        }

        public object Peek(string reg)
        {
            switch (reg)
            {
                case "eax":
                    return _eax.Peek();
                case "ebx":
                    return _ebx.Peek();
                default:
                    throw new ArgumentOutOfRangeException(nameof(reg));
            }
        }

        public object Pop(string reg)
        {
            switch (reg)
            {
                case "eax":
                    return _eax.Pop();
                case "ebx":
                    return _ebx.Pop();
                default:
                    throw new ArgumentOutOfRangeException(nameof(reg));
            }
        }
    }
}
