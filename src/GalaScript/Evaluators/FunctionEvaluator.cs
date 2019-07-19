using System.Linq;
using GalaScript.Abstract;

namespace GalaScript.Evaluators
{
    public class FunctionEvaluator : IEvaluator
    {
        private readonly IEngine _engine;
        private IScriptEvaluator _caller;

        private readonly string _name;
        private readonly IEvaluator[] _parameters;

        public FunctionEvaluator(IEngine engine, string name, IEvaluator[] parameters)
        {
            _engine = engine;
            _name = name;
            _parameters = parameters;
        }

        public void SetCaller(IScriptEvaluator caller)
        {
            _caller = caller;

            foreach (var parameter in _parameters)
            {
                parameter?.SetCaller(caller);
            }
        }

        public object Evaluate()
        {
            return _engine.Call(_caller, _name, _parameters?.Select(p => p?.Evaluate()).ToArray());
        }
    }
}
