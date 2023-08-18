using System;
using System.Collections.Generic;
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
        T[] GetComponents<T>() where T : IComponent;
        IComponent[] GetComponents(Type type);
    }

    class GameWorldHandle : IGameWorld
    {
        public string name { get; set; }
        public Camera camera { get; set; }

        private Dictionary<int, IEntity> entities = new Dictionary<int, IEntity>();
        private Dictionary<Type, List<IComponent>> map = new Dictionary<Type, List<IComponent>>();
        private Dictionary<int, List<IComponent>> components = new Dictionary<int, List<IComponent>>();

        public void Release()
        {
            foreach (var VARIABLE in entities.Values)
            {
                Engine.Class.Release(VARIABLE);
            }

            foreach (var VARIABLE in map.Values)
            {
                VARIABLE.ForEach(Engine.Class.Release);
            }

            map.Clear();
            entities.Clear();
            components.Clear();
            name = String.Empty;
            GameObject.DestroyImmediate(camera.gameObject);
            GC.SuppressFinalize(this);
        }

        public void OnDisable()
        {
            foreach (var VARIABLE in map.Values)
            {
                VARIABLE.ForEach(x => x.OnDisable());
            }

            camera.enabled = false;
        }

        public void OnEnable()
        {
            foreach (var VARIABLE in map.Values)
            {
                VARIABLE.ForEach(x => x.OnEnable());
            }

            camera.enabled = true;
        }

        public IEntity Find(int guid)
        {
            if (entities.TryGetValue(guid, out IEntity entity))
            {
                return entity;
            }

            return default;
        }

        public IEntity CreateEntity()
        {
            InternalEntityObject entityObject = Engine.Class.Loader<InternalEntityObject>();
            entityObject.guid = Guid.NewGuid().GetHashCode();
            entityObject.worldHandle = this;
            entities.Add(entityObject.guid, entityObject);
            return entityObject;
        }

        public void DestroyEntity(int guid)
        {
            entities.Remove(guid);
            if (!components.TryGetValue(guid, out List<IComponent> list))
            {
                return;
            }

            foreach (var VARIABLE in list)
            {
                if (!map.TryGetValue(VARIABLE.GetType(), out List<IComponent> temp))
                {
                    continue;
                }

                temp.Remove(VARIABLE);
                Engine.Class.Release(VARIABLE);
            }
        }

        public IComponent AddComponent(int guid, Type type)
        {
            if (typeof(EntityComponent).IsAssignableFrom(type) is false)
            {
                Engine.Console.Error(EngineException.Create<NotImplementedException>(type.Name));
                return default;
            }

            EntityComponent component = default;
            if (!components.TryGetValue(guid, out List<IComponent> list))
            {
                components.Add(guid, list = new List<IComponent>());
            }

            component = (EntityComponent)list.Find(x => x.GetType() == type);
            if (component is not null)
            {
                return component;
            }

            component = (EntityComponent)Engine.Class.Loader(type);
            component.entity = Find(guid);
            list.Add(component);
            if (!map.TryGetValue(type, out List<IComponent> temp))
            {
                map.Add(type, temp = new List<IComponent>());
            }

            temp.Add(component);
            return component;
        }

        public IComponent GetComponent(int guid, Type type)
        {
            if (typeof(IComponent).IsAssignableFrom(type) is false)
            {
                Engine.Console.Error(EngineException.Create<NotImplementedException>(type.Name));
                return default;
            }

            IComponent component = default;
            if (!components.TryGetValue(guid, out List<IComponent> list))
            {
                components.Add(guid, list = new List<IComponent>());
            }

            component = list.Find(x => x.GetType() == type);
            if (component is not null)
            {
                return component;
            }

            return default;
        }

        public IComponent[] GetComponents(int guid)
        {
            if (!components.TryGetValue(guid, out List<IComponent> list))
            {
                components.Add(guid, list = new List<IComponent>());
            }

            return list.ToArray();
        }

        public void DestroyComponent(int guid, Type type)
        {
            IComponent component = GetComponent(guid, type);
            if (component is null)
            {
                return;
            }

            if (components.TryGetValue(guid, out List<IComponent> list))
            {
                list.Remove(component);
            }

            if (map.TryGetValue(type, out list))
            {
                list.Remove(component);
            }
        }

        public T[] GetComponents<T>() where T : IComponent
        {
            if (!map.TryGetValue(typeof(T), out List<IComponent> list))
            {
                map.Add(typeof(T), list = new List<IComponent>());
            }

            T[] result = new T[list.Count];
            for (int i = 0; i < list.Count; i++)
            {
                result[i] = (T)list[i];
            }

            return result;
        }

        public IComponent[] GetComponents(Type type)
        {
            if (!map.TryGetValue(type, out List<IComponent> list))
            {
                map.Add(type, list = new List<IComponent>());
            }

            return list.ToArray();
        }
    }
}