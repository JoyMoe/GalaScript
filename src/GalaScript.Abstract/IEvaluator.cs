namespace GalaScript.Abstract
{
    public interface IEvaluator
    {
        void SetCaller(IScriptEvaluator caller);

        object Evaluate();
    }
}
