using GalaScript.Interfaces;

namespace GalaScript.Evaluators
{
    public class ConstantEvaluator : IEvaluator
    {
        private readonly string _k;

        public ConstantEvaluator(string k)
        {
            _k = k;
        }

        public void SetCaller(IScriptEvaluator caller)
        {
            //
        }

        public object Evaluate()
        {
            return _k;
        }
    }
}
