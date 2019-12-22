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
            return _k switch
            {
                var _k when _k.ToLower() == "true" => true,
                var _k when _k.ToLower() == "false" => false,
                var _k when _k.ToLower() == "null" => null,
                _ => _k,
            };
        }

        public override string ToScriptString() => _k;
    }
}
