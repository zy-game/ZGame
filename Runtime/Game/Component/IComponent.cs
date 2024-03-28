using System;

namespace ZGame.Game
{
    public class IComponent : IReferenceObject
    {
        public GameEntity entity;

        public virtual void OnAwake(params object[] args)
        {
        }

        public virtual void Release()
        {
        }
    }
}