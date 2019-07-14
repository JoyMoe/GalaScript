using System.Collections.Generic;
using System.Text;
using Sprache;

namespace GalaScript.Interfaces
{
    public interface IParser
    {
        void RegisterEvaluator(Parser<IEvaluator> parser);

        IEnumerable<IEvaluator> Prepare(string str);

        IScriptEvaluator LoadString(string str);

        IScriptEvaluator LoadFile(string file, Encoding encoding = null);
    }
}
