﻿using System;
using System.Text;

namespace GalaScript.Interfaces
{
    public delegate void EngineEventHandler();

    public interface IEngine
    {
        bool Debug { get; }

        bool Paused { get; set; }

        event EngineEventHandler OnStartedHandler;

        event EngineEventHandler OnPausedHandler;

        event EngineEventHandler OnResumedHandler;

        event EngineEventHandler OnExitedHandler;

        IParser Parser { get; set; }

        IScriptEvaluator Current { get; set; }

        void Register(string name, Delegate func);

        object Call(IScriptEvaluator caller, string name, params object[] arguments);

        void SetAlias(IScriptEvaluator caller, string name, object value);

        object GetAlias(IScriptEvaluator caller, string name);

        void Prepare(string str, Encoding encoding = null);

        void Reset();

        object Run();

        object Run(string str, Encoding encoding = null);
    }
}
