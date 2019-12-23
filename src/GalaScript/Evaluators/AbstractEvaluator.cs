using System;
using GalaScript.Interfaces;

namespace GalaScript.Evaluators
{
    public abstract class AbstractEvaluator : IEvaluator
    {
        protected IScriptEvaluator _caller;

        public virtual void SetCaller(IScriptEvaluator caller)
        {
            _caller = caller;
        }

        public virtual object Evaluate() => throw new NotImplementedException();

        public virtual string ToScriptString() => throw new NotImplementedException();

        public override string ToString() => ToScriptString();
    }
}
