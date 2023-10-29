using System;

namespace ZGame.State
{
    public interface IStateMachine : IDisposable
    {
        string name { get; }
        void Switch(Type type);
    }

    public interface IStateHandle : IDisposable
    {
        void OnCreate();
        void OnEntry();
        void OnExit();
        void OnUpdate();
    }
}