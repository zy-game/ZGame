using System;

namespace ZEngine.World
{
    public interface IEntity : IReference
    {
        int guid { get; }

        T AddComponent<T>() where T : IComponent
        {
            return (T)AddComponent(typeof(T));
        }

        T GetComponent<T>() where T : IComponent
        {
            return (T)GetComponent(typeof(T));
        }

        void DestroyComponent<T>()
        {
            DestroyComponent(typeof(T));
        }

        IComponent AddComponent(Type type);

        IComponent GetComponent(Type type);

        void DestroyComponent(Type type);
    }

    class InternalEntityObject : IEntity
    {
        public int guid { get; set; }
        public GameWorldHandle worldHandle;

        public void Release()
        {
            guid = 0;
            worldHandle = null;
            GC.SuppressFinalize(this);
        }

        public IComponent AddComponent(Type type)
        {
            return worldHandle.AddComponent(guid, type);
        }

        public IComponent GetComponent(Type type)
        {
            return worldHandle.GetComponent(guid, type);
        }

        public void DestroyComponent(Type type)
        {
            worldHandle.DestroyComponent(guid, type);
        }
    }
}