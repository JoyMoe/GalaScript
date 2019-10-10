using GalaScript.Interfaces;

namespace GalaScript.Evaluators
{
    public class LabelEvaluator : IEvaluator, INamedEvaluator
    {
        public LabelEvaluator(string label)
        {
            Name = label;
        }

        public void SetCaller(IScriptEvaluator caller)
        {
            //
        }

        public object Evaluate()
        {
            return Name;
        }

        public string Name { get; }
    }
}
