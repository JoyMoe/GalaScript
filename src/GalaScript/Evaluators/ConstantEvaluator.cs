using System.Collections.Generic;

namespace GalaScript.Evaluators
{
    public class ConstantEvaluator : AbstractEvaluator
    {
        private readonly Dictionary<string, object> _constant = new Dictionary<string, object>
        {
            ["true"] = true,
            ["false"] = false,
            ["null"] = null,
        };

        private readonly string _k;

        public ConstantEvaluator(string k)
        {
            _k = k.ToLower() switch
            {
                var l when _constant.ContainsKey(l) => l,
                _ => k,
            };
        }

        public override object Evaluate()
        {
            return _k switch
            {
                _ when _constant.TryGetValue(_k, out var c) => c,
                _ => _k,
            };
        }

        public override string ToScriptString() => _k;
    }
}
