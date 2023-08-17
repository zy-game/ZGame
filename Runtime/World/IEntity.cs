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

        IComponent AddComponent(Type type)
        {
            return WorldManager.instance.current.AddComponent(guid, type);
        }

        T GetComponent<T>() where T : IComponent
        {
            return (T)GetComponent(typeof(T));
        }

        IComponent GetComponent(Type type)
        {
            return WorldManager.instance.current.GetComponent(guid, type);
        }

        void DestroyComponent<T>()
        {
            DestroyComponent(typeof(T));
        }

        void DestroyComponent(Type type)
        {
            WorldManager.instance.current.DestroyComponent(guid, type);
        }
    }
}