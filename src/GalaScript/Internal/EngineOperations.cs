using GalaScript.Interfaces;

namespace GalaScript.Internal
{
    internal static partial class InternalOperations
    {
        public static class EngineOperations
        {
            public static string Print(char op, string text) => text;

            public static void Halt(IScriptEngine engine) => engine.Pause();

            public static void Push(IScriptEvaluator script) => script.Push();

            public static object Peek(IScriptEvaluator script) => script.Peek();

            public static object Pop(IScriptEvaluator script) => script.Pop();

            public static void Goto(IScriptEvaluator script, string label) => script.Goto(label);

            public static void Goif(IScriptEvaluator script, string label)
            {
                bool state = script.Return switch
                {
                    string @string => !string.IsNullOrWhiteSpace(@string),
                    decimal @decimal => (@decimal > 0),
                    _ => (script.Return != null)
                };

                if (state)
                {
                    script.Goto(label);
                }
            }
        }
    }
}
