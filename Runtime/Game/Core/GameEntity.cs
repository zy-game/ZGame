using System;
using System.Collections.Generic;
using UnityEngine;

namespace ZGame.Game
{
    /// <summary>
    /// 游戏实体对象
    /// </summary>
    public sealed class GameEntity : IReference
    {
        private uint _id;
        private World _world;

        /// <summary>
        /// 实体id
        /// </summary>
        public uint id
        {
            get { return _id; }
        }

        public World owner
        {
            get { return _world; }
        }

        internal static GameEntity Create(World world, uint id = 0)
        {
            GameEntity gameEntity = RefPooled.Alloc<GameEntity>();
            gameEntity._id = id == 0 ? Crc32.GetCRC32Str(Guid.NewGuid().ToString()) : id;
            gameEntity._world = world;
            return gameEntity;
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
            return _world.AddComponent(id, type);
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
            return _world.GetComponent(id, type);
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
            _world.RemoveComponent(id, type);
        }

        public bool TryGetComponent<T>(out T component) where T : IComponent
        {
            component = default;
            var state = TryGetComponent(typeof(T), out IComponent result);
            if (state is false)
            {
                return false;
            }

            component = (T)result;
            return true;
        }

        public bool TryGetComponent(Type type, out IComponent component)
        {
            return _world.TryGetComponent(id, type, out component);
        }

        public void Release()
        {
            _id = 0;
        }
    }
}