using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using GalaScript.Interfaces;
using GalaScript.Internal;

namespace GalaScript.Evaluators
{
    public class ScriptEvaluator : IScriptEvaluator
    {
        protected readonly IEngine Engine;

        private LinkedListNode<IEvaluator> _currentNode;
        protected readonly LinkedList<IEvaluator> Script = new LinkedList<IEvaluator>();
        protected readonly Dictionary<string, KeyValuePair<long, LinkedListNode<IEvaluator>>> Labels =
            new Dictionary<string, KeyValuePair<long, LinkedListNode<IEvaluator>>>();

        private IDropOutStack<object> _eax = new DropOutStack<object>(10);
        private IDropOutStack<object> _ebx = new DropOutStack<object>(10);
        private Dictionary<string, object> _aliases = new Dictionary<string, object>();

        protected ScriptEvaluator(IEngine engine)
        {
            Engine = engine;
        }

        public ScriptEvaluator(IEngine engine, string str) : this(engine)
        {
            var evaluators = Engine.Parser.Prepare(str);

            foreach (var exp in evaluators)
            {
                if (Engine.Debug == false && exp == null) continue;

                exp?.SetCaller(this);

                Script.AddLast(exp);

                CurrentLineNumber++;

                switch (Script.Last.Value)
                {
                    case MacroEvaluator macro:
                        macro.ReplaceEnvironment(_aliases);

                        object Macro(object[] objects)
                        {
                            macro.SetCaller(objects.FirstOrDefault() as IScriptEvaluator);
                            macro.SetArguments(objects.ToArray());

                            return macro.Evaluate();
                        }

                        Engine.Register(macro.Name, (Func<object[], object>) Macro);

                        break;
                    case ScriptEvaluator sub:
                        // TODO: add option to config ReplaceEnvironment in sub-script
                        sub.ReplaceEnvironment(ref _eax, ref _ebx, ref _aliases);
                        break;
                    case LabelEvaluator label:
                        Labels[label.Name] =
                            new KeyValuePair<long, LinkedListNode<IEvaluator>>(CurrentLineNumber, Script.Last);
                        break;
                }
            }

            Reset();
        }

        public void SetCaller(IScriptEvaluator caller)
        {
            // TODO: Set caller for Script
        }

        public long CurrentLineNumber { get; protected set; }

        public IEvaluator Current => _currentNode?.Value;

        public IDropOutStack<object> Eax => _eax;

        public IDropOutStack<object> Ebx => _ebx;

        public Dictionary<string, object> Aliases => _aliases;

        public object Return => GetAlias("ret");

        public void Goto(string label)
        {
            if (Labels.TryGetValue(label, out var kv))
            {
                CurrentLineNumber = kv.Key;
                _currentNode = kv.Value;
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
                    _currentNode = Script.First;
                    CurrentLineNumber = offset;
                    break;
                case SeekOrigin.Current:
                    CurrentLineNumber += offset;
                    break;
                case SeekOrigin.End:
                    _currentNode = Script.Last;
                    CurrentLineNumber = Script.Count + offset;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(origin), origin, null);
            }

            if (offset > 0)
            {
                for (var i = 0; i < offset; i++)
                {
                    _currentNode = _currentNode?.Next;
                }
            }
            else
            {
                for (var i = 0; i > offset; i--)
                {
                    _currentNode = _currentNode?.Previous;
                }
            }
        }

        public void Reset()
        {
            if (Labels.ContainsKey("start"))
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
            if (Current != null && Current is MacroEvaluator == false)
            {
                Current.Evaluate();
            }

            Seek(1, SeekOrigin.Current);

            return Return;
        }

        public object Evaluate()
        {
            while (_currentNode != null)
            {
                Engine.Current = this;

                while (Engine.Debug && Engine.Paused)
                {
                    Thread.Sleep(5);
                }

                StepOut();
            }

            return Return;
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
                    _eax.Push(Return);
                    break;
                case "ebx":
                    _ebx.Push(Return);
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
