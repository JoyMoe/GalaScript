using System.Linq;
using GalaScript.Interfaces;

namespace GalaScript.Evaluators
{
    public class FunctionEvaluator : AbstractEvaluator
    {
        private readonly IScriptEngine _engine;

        private readonly string _name;
        private readonly IEvaluator[] _parameters;

        public FunctionEvaluator(IScriptEngine engine, string name, IEvaluator[] parameters)
        {
            _engine = engine;
            _name = name;
            _parameters = parameters;
        }

        public override void SetCaller(IScriptEvaluator caller)
        {
            foreach (var parameter in _parameters)
            {
                parameter?.SetCaller(caller);
            }

            base.SetCaller(caller);
        }

        public override object Evaluate()
        {
            return _engine.Call(_caller, _name, _parameters?.Select(p => p?.Evaluate()).ToArray());
        }

        public override string ToScriptString()
        {
            var @string = $"[{_name}";
            var parameters = _parameters?.Select(p => p?.ToString());

            if (parameters != null)
            {
                @string += $" {string.Join(" ", parameters)}";
            }

            return @string + "]";
        }
    }
}
