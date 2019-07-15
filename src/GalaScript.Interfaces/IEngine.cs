using System;
using System.Collections.Generic;
using System.Text;

namespace GalaScript.Interfaces
{
    public interface IEngine
    {
        IParser Parser { get; set; }

        void Register(string name, Delegate func);

        object Call(IScriptEvaluator caller, string name, params object[] arguments);

        void SetAlias(IScriptEvaluator caller, string name, object value);

        object GetAlias(IScriptEvaluator caller, string name);

        void Prepare(string str, Encoding encoding = null);

        object Evaluate();

        object Evaluate(string str, Encoding encoding = null);
    }
}
