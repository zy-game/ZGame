using System;
using UnityEngine;

namespace ZEngine.World
{
    public interface IGameWorld : IReference
    {
        void Awake(params object[] paramsList);
        void Update();
        void FixedUpdate();
        void LateUpdate();
        void OnDisable();
        void OnEnable();
        void LoadLogicSystem(Type systemType);
        void LoadLogicSystem<T>() where T : ILogicSystemExecuteHandle;
        void UnloadLogicSystem(Type systemType);
        void UnloadLogicSystem<T>() where T : ILogicSystemExecuteHandle;
        ILogicSystemExecuteHandle SwitchSystem<T>(params object[] paramsList) where T : ILogicSystemExecuteHandle;
    }

    public interface IGameWorldOptions : IReference
    {
        string name { get; }
        Camera camera { get; }
        string sceneName { get; }
    }
}