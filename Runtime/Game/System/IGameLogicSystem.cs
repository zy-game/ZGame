using System;

namespace ZGame.Game
{
    public interface IGameLogicSystem : IReferenceObject
    {
        void OnAwakw(params object[] args);
        void OnLateUpdate();
        void OnFixedUpdate();
        void OnUpdate();
    }
}