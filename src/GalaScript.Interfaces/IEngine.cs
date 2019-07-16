using System;
using System.Text;

namespace GalaScript.Interfaces
{
    public interface IEngine
    {
        bool Debug { get; }

        bool Paused { get; set; }

        IParser Parser { get; set; }

        IScriptEvaluator Current { get; set; }

        void Register(string name, Delegate func);

        object Call(IScriptEvaluator caller, string name, params object[] arguments);

        void SetAlias(IScriptEvaluator caller, string name, object value);

        object GetAlias(IScriptEvaluator caller, string name);

        void Prepare(string str, Encoding encoding = null);

        object Run();

        object Run(string str, Encoding encoding = null);
    }
}
