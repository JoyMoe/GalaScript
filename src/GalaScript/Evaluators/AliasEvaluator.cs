using GalaScript.Interfaces;

namespace GalaScript.Evaluators
{
    public class AliasEvaluator : IEvaluator
    {
        private readonly IScriptEngine _engine;
        private IScriptEvaluator _caller;

        private readonly string _name;
        private readonly IEvaluator _value;

        public AliasEvaluator(IScriptEngine engine, string name, IEvaluator value = null)
        {
            _engine = engine;
            _name = name;
            _value = value;
        }

        public void SetCaller(IScriptEvaluator caller)
        {
            _caller = caller;

            _value?.SetCaller(caller);
        }

        public object Evaluate()
        {
            if (_value == null)
            {
                return _engine.GetAlias(_caller, _name);
            }

            var result = _value.Evaluate();

            _engine.SetAlias(_caller, _name, result);

            return result;
        }
    }
}
