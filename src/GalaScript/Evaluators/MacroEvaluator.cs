using System.Collections.Generic;
using System.Linq;
using GalaScript.Interfaces;

namespace GalaScript.Evaluators
{
    public class MacroEvaluator : ScriptEvaluator, INamedEvaluator
    {
        private readonly string _name;
        private readonly IEnumerable<string> _parameters;

        public MacroEvaluator(IEngine engine, string name, IEnumerable<string> parameters, string str) : base(engine, str, false)
        {
            _name = name;
            _parameters = parameters;
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
