using GalaScript.Interfaces;

namespace GalaScript.Internal
{
    internal static class EngineOperations
    {
        public static void Push(IScriptEvaluator script, string reg)
        {
            script.Push(reg);
        }

        public static object Peek(IScriptEvaluator script, string reg)
        {
            return script.Peek(reg);
        }

        public static object Pop(IScriptEvaluator script, string reg)
        {
            return script.Pop(reg);
        }

        public static void Goto(IScriptEvaluator script, string label)
        {
            script.Goto(label);
        }

        public static void Goif(IScriptEvaluator script, string label)
        {
            bool state;

            switch (script.Return)
            {
                case string @string:
                    state = !string.IsNullOrWhiteSpace(@string);
                    break;
                case decimal @decimal:
                    state = @decimal > 0;
                    break;
                default:
                    state = script.Return != null;
                    break;
            }

            if (state)
            {
                script.Goto(label);
            }
        }
    }
}
