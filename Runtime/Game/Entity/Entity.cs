using System;
using System.Collections.Generic;
using UnityEngine;
using ZGame.Resource;

namespace ZGame.Game
{
    /// <summary>
    /// 游戏实体对象
    /// </summary>
    public sealed class Entity : IReferenceObject
    {
        private uint _id;

        /// <summary>
        /// 实体id
        /// </summary>
        public uint id => _id;


        internal static Entity Create()
        {
            Entity entity = GameFrameworkFactory.Spawner<Entity>();
            entity._id = Crc32.GetCRC32Str(Guid.NewGuid().ToString());
            return entity;
        }

        /// <summary>
        /// 添加组件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T AddComponent<T>() where T : IComponent
        {
            return (T)AddComponent(typeof(T));
        }

        /// <summary>
        /// 添加组件
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public IComponent AddComponent(Type type)
        {
            return GameFrameworkEntry.ECS.AddComponent(this, type);
        }

        /// <summary>
        /// 获取组件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetComponent<T>() where T : IComponent
        {
            return (T)GetComponent(typeof(T));
        }

        /// <summary>
        /// 获取组件
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public IComponent GetComponent(Type type)
        {
            return GameFrameworkEntry.ECS.GetComponent(this, type);
        }

        /// <summary>
        /// 移除组件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void RemoveComponent<T>() where T : IComponent
        {
            RemoveComponent(typeof(T));
        }

        /// <summary>
        /// 移除组件
        /// </summary>
        /// <param name="type"></param>
        /// <exception cref="NotImplementedException"></exception>
        public void RemoveComponent(Type type)
        {
            GameFrameworkEntry.ECS.RemoveComponent(this, type);
        }

        public void Release()
        {
            _id = 0;
        }
    }
}