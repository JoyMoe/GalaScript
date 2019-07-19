using System.Collections.Generic;
using System.IO;

namespace GalaScript.Abstract
{
    public interface IScriptEvaluator : IEvaluator
    {
        long CurrentLineNumber { get; }

        IEvaluator Current { get; }

        Stack<object> Stack { get; }

        Dictionary<string, object> Aliases { get; }

        object Return { get; }

        void Goto(string label);

        void Seek(long offset, SeekOrigin origin);

        void Reset();

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
