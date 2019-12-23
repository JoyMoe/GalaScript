namespace GalaScript.Evaluators
{
    public class StringConstantEvaluator : AbstractEvaluator
    {
        private readonly string _string;

        public StringConstantEvaluator(string @string)
        {
            _string = @string;
        }

        public override object Evaluate()
        {
            return _string;
        }

        public override string ToScriptString() => $"\"{_string}\"";
    }
}
