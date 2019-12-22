using System.Globalization;

namespace GalaScript.Evaluators
{
    public class IntegerConstantEvaluator : AbstractEvaluator
    {
        private readonly long _integer;

        public IntegerConstantEvaluator(long integer)
        {
            _integer = integer;
        }

        public override object Evaluate()
        {
            return _integer;
        }

        public override string ToScriptString() => $"{_integer.ToString(CultureInfo.InvariantCulture)}L";
    }
}
