using GalaScript.Abstract;

namespace GalaScriptTests
{
    public class JurassicEvaluator : IEvaluator
    {
        private readonly string _js;

        public JurassicEvaluator(string js)
        {
            _js = js;
        }

        public void SetCaller(IScriptEvaluator caller)
        {
            //
        }

        public object Evaluate()
        {
            var engine = new Jurassic.ScriptEngine();

            engine.Execute(_js);

            return engine.GetGlobalValue<string>("result");
        }
    }
}
