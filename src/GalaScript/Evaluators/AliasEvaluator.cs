using GalaScript.Interfaces;

namespace GalaScript.Evaluators
{
    public class AliasEvaluator : AbstractEvaluator
    {
        private readonly IScriptEngine _engine;

        private readonly string _name;
        private readonly IEvaluator _value;

        public AliasEvaluator(IScriptEngine engine, string name, IEvaluator value = null)
        {
            _engine = engine;
            _name = name;
            _value = value;
        }

        public override void SetCaller(IScriptEvaluator caller)
        {
            _value?.SetCaller(caller);

            base.SetCaller(caller);
        }

        public override object Evaluate()
        {
            if (_value == null)
            {
                return _engine.GetAlias(_caller, _name);
            }

            var result = _value.Evaluate();

            _engine.SetAlias(_caller, _name, result);

            return result;
        }

        public override string ToScriptString() => _value == null ? $"{_name}" : $"{_value} : {_name}";
    }
}
