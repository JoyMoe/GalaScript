using GalaScript.Interfaces;

namespace GalaScript.Evaluators
{
    public class LabelEvaluator : AbstractEvaluator, INamedEvaluator
    {
        public LabelEvaluator(string label)
        {
            Name = $"* {label}";
        }

        public override object Evaluate()
        {
            return Name;
        }

        public string Name { get; }

        public override string ToScriptString() => Name;
    }
}
