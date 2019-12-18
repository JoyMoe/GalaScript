using GalaScript.Interfaces;

namespace GalaScript.Evaluators
{
    public class IntegerConstantEvaluator : IConstantEvaluator
    {
        private readonly decimal _long;

        public IntegerConstantEvaluator(long @long)
        {
            _long = @long;
        }

        public void SetCaller(IScriptEvaluator caller)
        {
            //
        }

        public object Evaluate()
        {
            return _long;
        }
    }
}
