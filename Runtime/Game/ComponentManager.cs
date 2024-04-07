using System;
using System.Collections.Generic;

namespace ZGame.Game
{
    /// <summary>
    /// 组件管理器
    /// </summary>
    class ComponentManager : IReferenceObject
    {
        private List<ComponentChunk> chunkList = new List<ComponentChunk>();


        public void Release()
        {
            Clear();
        }

        private void EnsureComponentType(Type type)
        {
            if (typeof(IComponent).IsAssignableFrom(type) is false)
            {
                throw new NotImplementedException("类型必须是IComponent的子类");
            }
        }

        private ComponentChunk GetComponentChunk(Type type)
        {
            EnsureComponentType(type);
            ComponentChunk componentChunk = chunkList.Find(x => type == x.owner);
            if (componentChunk is null)
            {
                chunkList.Add(componentChunk = ComponentChunk.Create(type));
            }

            return componentChunk;
        }

        /// <summary>
        /// 添加组件
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public IComponent AddComponent(uint id, Type type)
        {
            EnsureComponentType(type);
            return GetComponentChunk(type).AddComponent(id);
        }

        /// <summary>
        /// 获取组件
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public IComponent GetComponent(uint id, Type type)
        {
            EnsureComponentType(type);
            return GetComponentChunk(type).GetComponent(id);
        }

        /// <summary>
        /// 移除组件
        /// </summary>
        /// <param name="type"></param>
        /// <exception cref="NotImplementedException"></exception>
        public void RemoveComponent(uint id, Type type)
        {
            EnsureComponentType(type);
            GetComponentChunk(type).RemoveComponent(id);
        }

        /// <summary>
        /// 获取所有指定类型的组件
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public IComponent[] GetComponents(Type type)
        {
            EnsureComponentType(type);
            return GetComponentChunk(type).GetComponents();
        }

        /// <summary>
        /// 获取拥有指定类型组件的实体ID
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public uint[] GetHaveComponentEntityID(Type type)
        {
            EnsureComponentType(type);
            return GetComponentChunk(type).entitys;
        }

        /// <summary>
        /// 移除实体所有组件
        /// </summary>
        /// <param name="id"></param>
        public void RemoveEntityComponents(uint id)
        {
            foreach (var VARIABLE in chunkList)
            {
                VARIABLE.RemoveComponent(id);
            }
        }

        public void Clear()
        {
            chunkList.ForEach(GameFrameworkFactory.Release);
            chunkList.Clear();
        }
    }
}