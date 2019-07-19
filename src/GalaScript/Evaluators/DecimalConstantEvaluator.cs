using GalaScript.Abstract;

namespace GalaScript.Evaluators
{
    public class DecimalConstantEvaluator : IEvaluator
    {
        private readonly decimal _decimal;

        public DecimalConstantEvaluator(decimal @decimal)
        {
            _decimal = @decimal;
        }

        public void SetCaller(IScriptEvaluator caller)
        {
            //
        }

        public object Evaluate()
        {
            return _decimal;
        }
    }
}
