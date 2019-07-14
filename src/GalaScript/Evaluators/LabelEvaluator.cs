using GalaScript.Interfaces;

namespace GalaScript.Evaluators
{
    public class LabelEvaluator : IEvaluator, INamedEvaluator
    {
        private readonly string _label;

        public LabelEvaluator(string label)
        {
            _label = label;
        }

        public void SetCaller(IScriptEvaluator caller)
        {
            //
        }

        public object Evaluate()
        {
            return _label;
        }

        public string GetName()
        {
            return _label;
        }
    }
}
