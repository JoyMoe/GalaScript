using System;
using GalaScript.Interfaces;

namespace GalaScript.Evaluators
{
    public class MacroEvaluator : ScriptEvaluator, INamedEvaluator
    {
        private readonly string[] _parameters;

        public MacroEvaluator(IEngine engine, string name, string[] parameters, string str)
        {
            Name = name;
            _parameters = parameters;

            var evaluators = engine.Parser.Prepare(str);

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
                        _labels[label.Name] = _script.Last;
                        break;
                }

                _script.AddLast(exp);
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
