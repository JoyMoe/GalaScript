using System.Globalization;

namespace GalaScript.Evaluators
{
    public class DecimalConstantEvaluator : AbstractEvaluator
    {
        private readonly decimal _decimal;

        public DecimalConstantEvaluator(decimal @decimal)
        {
            _decimal = @decimal;
        }

        public override object Evaluate()
        {
            return _decimal;
        }

        public override string ToScriptString() => _decimal.ToString(CultureInfo.InvariantCulture);
    }
}
