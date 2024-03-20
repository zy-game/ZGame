using System;

namespace ZGame.Game
{
    public class EntityComponent : IDisposable
    {
        public GameEntity entity { get; private set; }

        internal void SetEntity(GameEntity entity)
        {
            this.entity = entity;
        }

        public virtual void OnAwake(params object[] args)
        {
        }

        public virtual void OnEnable()
        {
        }

        public virtual void OnUpdate()
        {
        }

        public virtual void OnFixedUpdate()
        {
        }

        public virtual void OnLateUpdate()
        {
        }

        public virtual void OnDisable()
        {
        }

        public virtual void Dispose()
        {
        }
    }
}