namespace GalaScript.Interfaces
{
    public interface IEvaluator
    {
        void SetCaller(IScriptEvaluator caller);

        object Evaluate();
    }
}
