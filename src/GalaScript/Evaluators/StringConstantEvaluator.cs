using GalaScript.Interfaces;

namespace GalaScript.Evaluators
{
    public class StringConstantEvaluator : IConstantEvaluator
    {
        private readonly string _string;

        public StringConstantEvaluator(string @string)
        {
            _string = @string;
        }

        public void SetCaller(IScriptEvaluator caller)
        {
            //
        }

        public object Evaluate()
        {
            return _string;
        }
    }
}
