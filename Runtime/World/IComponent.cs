using System;

namespace ZEngine.World
{
    public interface IComponent : IReference
    {
        IEntity entity { get; }
        void OnDisable();
        void OnEnable();
    }

    public abstract class EntityComponent : IComponent
    {
        public IEntity entity { get; internal set; }

        public virtual void Release()
        {
            entity = null;
            GC.SuppressFinalize(this);
        }

        public virtual void OnDisable()
        {
        }

        public virtual void OnEnable()
        {
        }
    }
}