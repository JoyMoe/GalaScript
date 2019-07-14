using GalaScript.Interfaces;

namespace GalaScript.Evaluators
{
    public class TextEvaluator : IEvaluator
    {
        private readonly string _text;
        private readonly bool _replace;

        public TextEvaluator(string text, bool replace = true)
        {
            _text = text?.Trim() ?? string.Empty;
            _replace = replace;
        }

        public void SetCaller(IScriptEvaluator caller)
        {
            //
        }

        public object Evaluate()
        {
            return _text;
        }
    }
}
