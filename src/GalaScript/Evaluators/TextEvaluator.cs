using GalaScript.Interfaces;

namespace GalaScript.Evaluators
{
    public class TextEvaluator : AbstractEvaluator
    {
        private readonly IScriptEngine _engine;

        private readonly string _text;
        private readonly char _op;

        public TextEvaluator(IScriptEngine engine, string text, char op)
        {
            _engine = engine;
            _text = text?.Trim() ?? string.Empty;
            _op = op;
        }

        public override object Evaluate()
        {
            return _engine.Call(_caller, "print", _op, _text);
        }

        public override string ToScriptString() => $"{_op} {_text}";
    }
}
