using System;
using System.Collections.Generic;
using System.Linq;
using GalaScript.Interfaces;

namespace GalaScript.Evaluators
{
    public class MacroEvaluator : ScriptEvaluator, INamedEvaluator
    {
        private readonly string _name;
        private readonly IEnumerable<string> _parameters;

        public MacroEvaluator(IEngine engine, string name, IEnumerable<string> parameters, string str)
        {
            _name = name;
            _parameters = parameters;

            var evaluators = engine.GetParser().Prepare(str);

            foreach (var exp in evaluators)
            {
                if (exp == null) continue;

                exp.SetCaller(this);

                switch (exp)
                {
                    case MacroEvaluator _:
                    case ScriptEvaluator _:
                        throw new NotSupportedException("Nested Macro is not supported.");
                    case LabelEvaluator label:
                        _labels[label.GetName()] = _script.Last;
                        break;
                }

                _script.AddLast(exp);
            }

            Reset();
        }

        public string GetName()
        {
            return _name;
        }

        public void SetArguments(IEnumerable<object> arguments)
        {
            for (var i = 0; i < _parameters.Count(); i++)
            {
                SetAlias(_parameters.ElementAt(i), arguments.ElementAt(i));
            }
        }
    }
}
