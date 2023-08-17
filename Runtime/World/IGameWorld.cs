using System;
using UnityEngine;

namespace ZEngine.World
{
    public interface IGameWorld : IReference
    {
        string name { get; }
        Camera camera { get; }
        void OnDisable();
        void OnEnable();
        IEntity Find(int guid);
        IEntity CreateEntity();
        void DestroyEntity(int guid);
        IComponent AddComponent(int guid, Type type);
        IComponent GetComponent(int guid, Type type);
        IComponent[] GetComponents(int guid);
        T[] GetComponents<T>() where T : IComponent;
        IComponent[] GetComponents(Type type);
        void DestroyComponent(int guid, Type type);
        void LoadLogicSystem(Type systemType);
        void UnloadLogicSystem(Type systemType);
    }

    class InternalGameWorldHandle : IGameWorld
    {
        public string name { get; set; }
        public Camera camera { get; set; }

        public void Release()
        {
        }

        public void OnDisable()
        {
        }

        public void OnEnable()
        {
        }

        public IEntity Find(int guid)
        {
            throw new NotImplementedException();
        }

        public IEntity CreateEntity()
        {
            throw new NotImplementedException();
        }

        public void DestroyEntity(int guid)
        {
            throw new NotImplementedException();
        }

        public void LoadLogicSystem(Type systemType)
        {
        }

        public void UnloadLogicSystem(Type systemType)
        {
        }

        public IComponent AddComponent(int guid, Type type)
        {
            return default;
        }

        public IComponent GetComponent(int guid, Type type)
        {
            return default;
        }

        public IComponent[] GetComponents(int guid)
        {
            throw new NotImplementedException();
        }

        public T[] GetComponents<T>() where T : IComponent
        {
            throw new NotImplementedException();
        }

        public IComponent[] GetComponents(Type type)
        {
            throw new NotImplementedException();
        }

        public void DestroyComponent(int guid, Type type)
        {
        }
    }
}