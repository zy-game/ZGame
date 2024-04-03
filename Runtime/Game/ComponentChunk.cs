using System;
using System.Collections.Generic;
using System.Linq;

namespace ZGame.Game
{
    public partial class ComponentChunk : IReferenceObject
    {
        private List<uint> idList;
        private List<IComponent> components;

        public uint[] entitys => idList.ToArray();

        public Type owner { get; private set; }


        public static ComponentChunk Create(Type owner)
        {
            ComponentChunk componentChunk = GameFrameworkFactory.Spawner<ComponentChunk>();
            componentChunk.components = new();
            componentChunk.idList = new();
            componentChunk.owner = owner;
            return componentChunk;
        }

        public void Release()
        {
            components.ForEach(GameFrameworkFactory.Release);
            idList.Clear();
            components.Clear();
        }

        public IComponent[] GetComponents()
        {
            return components.ToArray();
        }

        /// <summary>
        /// 添加组件
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public IComponent AddComponent(uint id)
        {
            IComponent component = default;
            component = GetComponent(id);
            if (component is not null)
            {
                return component;
            }

            component = (IComponent)GameFrameworkFactory.Spawner(owner);
            idList.Add(id);
            components.Add(component);
            return component;
        }

        /// <summary>
        /// 获取组件
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public IComponent GetComponent(uint id)
        {
            if (typeof(ISingletonComponent).IsAssignableFrom(owner))
            {
                return components.FirstOrDefault();
            }

            int index = idList.FindIndex(x => x == id);
            if (index != -1)
            {
                return components[index];
            }

            return default;
        }

        /// <summary>
        /// 移除组件
        /// </summary>
        /// <param name="type"></param>
        /// <exception cref="NotImplementedException"></exception>
        public void RemoveComponent(uint id)
        {
            if (typeof(ISingletonComponent).IsAssignableFrom(owner))
            {
                idList.Remove(id);
                return;
            }

            IComponent component = GetComponent(id);
            if (component is null)
            {
                return;
            }

            components.Remove(component);
            idList.Remove(id);
            GameFrameworkFactory.Release(component);
        }
    }
}