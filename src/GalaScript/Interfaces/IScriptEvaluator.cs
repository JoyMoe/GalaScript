using System.Collections.Generic;
using System.IO;

namespace GalaScript.Interfaces
{
    public interface IScriptEvaluator : Interfaces.IEvaluator
    {
        long CurrentLineNumber { get; }

        Interfaces.IEvaluator Current { get; }

        Stack<object> Stack { get; }

        Dictionary<string, object> Aliases { get; }

        object Return { get; }

        void Goto(string label);

        void Seek(long offset, SeekOrigin origin);

        void Reset();

        object StepIn();

        void ReplaceEnvironment(Dictionary<string, object> aliases = null);

        void ReplaceEnvironment(ref Dictionary<string, object> aliases);

        void ReplaceEnvironment(ref Stack<object> stack, ref Dictionary<string, object> aliases);

        void SetAlias(string name, object value);

        object GetAlias(string name);

        void Push();

        object Peek();

        object Pop();
    }
}
