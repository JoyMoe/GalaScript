using GalaScript.Evaluators;

namespace GalaScriptTests
{
    public class JurassicEvaluator : AbstractEvaluator
    {
        private readonly string _js;

        public JurassicEvaluator(string js)
        {
            _js = js;
        }

        public override object Evaluate()
        {
            var engine = new Jurassic.ScriptEngine();

            engine.Execute(_js);

            return engine.GetGlobalValue<string>("result");
        }
    }
}
