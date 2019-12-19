namespace GalaScript.Evaluators
{
    public class ConstantEvaluator : AbstractEvaluator
    {
        private readonly string _k;

        public ConstantEvaluator(string k)
        {
            _k = k;
        }

        public override object Evaluate()
        {
            return _k;
        }

        public override string ToScriptString() => _k;
    }
}
