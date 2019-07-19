using System;
using System.Text;

namespace GalaScript.Abstract
{
    public interface IEngine
    {

        bool IsCancellationRequested { get; }

        bool IsDebugAllowed { get; }

        bool IsStepInRequested { get; }

        bool IsPauseRequested { get; }

        bool Paused { get; }

        event EngineEventHandler OnStartedHandler;

        event EngineEventHandler OnPausedHandler;

        event EngineEventHandler OnResumedHandler;

        event EngineEventHandler OnExitedHandler;

        IParser Parser { get; set; }

        IScriptEvaluator Current { get; }

        void Register(string name, Delegate func);

        object Call(IScriptEvaluator caller, string name, params object[] arguments);

        void SetAlias(IScriptEvaluator caller, string name, object value);

        object GetAlias(IScriptEvaluator caller, string name);

        void Prepare(string str, Encoding encoding = null);

        void Reset();

        object Run();

        object Run(string str, Encoding encoding = null);

        void Cancel();

        void Continue();

        void Pause();

        void StepIn();
    }
}
