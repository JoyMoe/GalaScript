using GalaScript.Interfaces;

namespace GalaScript.Evaluators
{
    public class TextEvaluator : IEvaluator
    {
        private readonly IScriptEngine _engine;
        private IScriptEvaluator _caller;

        private readonly string _text;
        private readonly char _op;

        public TextEvaluator(IScriptEngine engine, string text, char op)
        {
            _engine = engine;
            _text = text?.Trim() ?? string.Empty;
            _op = op;
        }

        public void SetCaller(IScriptEvaluator caller)
        {
            _caller = caller;
        }

        public object Evaluate()
        {
            return _engine.Call(_caller, "print", _op, _text);
        }
    }
}
