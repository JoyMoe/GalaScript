using System;
using System.Collections.Generic;
using GalaScript.Abstract;

namespace GalaScript.Evaluators
{
    public class MacroEvaluator : ScriptEvaluator, INamedEvaluator
    {
        private readonly string[] _parameters;

        public MacroEvaluator(ScriptEngine engine, string name, string[] parameters, string str) : base(engine)
        {
            Name = name;
            _parameters = parameters ?? new string[0];

            var evaluators = Engine.Parser.Prepare(str);

            foreach (var exp in evaluators)
            {
                if (Engine.IsDebugAllowed == false && exp == null) continue;

                exp?.SetCaller(this);

                Script.AddLast(exp);

                CurrentLineNumber++;

                switch (Script.Last.Value)
                {
                    case MacroEvaluator _:
                    case ScriptEvaluator _:
                        throw new NotSupportedException("Nested Macro is not supported.");
                    case LabelEvaluator label:
                        Labels[label.Name] =
                            new KeyValuePair<long, LinkedListNode<IEvaluator>>(CurrentLineNumber, Script.Last);
                        break;
                }
            }

            Reset();
        }

        public void SetArguments(object[] arguments)
        {
            for (var i = 0; i < _parameters.Length; i++)
            {
                SetAlias(_parameters[i], arguments[i]);
            }
        }

        public string Name { get; }
    }
}
