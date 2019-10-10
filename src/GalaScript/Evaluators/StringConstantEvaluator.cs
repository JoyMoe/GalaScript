using GalaScript.Interfaces;

namespace GalaScript.Evaluators
{
    public class StringConstantEvaluator : IEvaluator
    {
        private readonly string _text;

        public StringConstantEvaluator(string text)
        {
            _text = text;
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
