using System.Collections.Generic;
using GalaScript.Interfaces;

namespace GalaScript.Evaluators
{
    public class NamedParameterEvaluator : AbstractEvaluator, INamedEvaluator
    {
        private IEvaluator _evaluator;

        public NamedParameterEvaluator(string name, IEvaluator evaluator)
        {
            Name = name;
            _evaluator = evaluator;
        }

        public override object Evaluate()
        {
            return new KeyValuePair<string, object>(Name, _evaluator.Evaluate());
        }

        public string Name { get; }

        public override string ToScriptString() => $"{Name}={_evaluator.ToString()}";
    }
}
