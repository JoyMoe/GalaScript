using System.Collections.Generic;
using System.IO;

namespace GalaScript.Interfaces
{
    public interface IScriptEvaluator : IEvaluator
    {
        object Return { get; }

        void Goto(string label);

        void Seek(long offset, SeekOrigin origin);

        void Reset();

        object StepOut();

        void ReplaceEnvironment(Dictionary<string, object> aliases = null);

        void ReplaceEnvironment(ref Dictionary<string, object> aliases);

        void ReplaceEnvironment(ref IDropOutStack<object> eax, ref IDropOutStack<object> ebx,
            ref Dictionary<string, object> aliases);

        void SetAlias(string name, object value);

        object GetAlias(string name);

        void Push(string reg);

        object Peek(string reg);

        object Pop(string reg);
    }
}
