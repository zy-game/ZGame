using System;
using System.Collections.Generic;
using UnityEngine;

namespace ZGame
{
    /// <summary>
    /// 引用工厂
    /// </summary>
    public class RefPooled
    {
        private static readonly List<RefHandle> _queue = new();

        /// <summary>
        /// 获取或创建引用对象句柄
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private static RefHandle GetOrCreareReferenceHandle(Type type)
        {
            RefHandle handle = _queue.Find(x => x.owner == type);
            if (handle == null)
            {
                handle = new(type);
                _queue.Add(handle);
            }

            return handle;
        }

        /// <summary>
        /// 创建一个引用对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T Alloc<T>(params object[] args) where T : IReference
        {
            return (T)Alloc(typeof(T), args);
        }

        /// <summary>
        /// 创建一个引用对象
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static IReference Alloc(Type type, params object[] args)
        {
            if (typeof(IReference).IsAssignableFrom(type) is false)
            {
                throw new ArgumentException("type must be IGameReferenceObject");
            }

            // Debug.Log("Spawner Reference Object:" + type);
            return GetOrCreareReferenceHandle(type).Spawner(args);
        }

        /// <summary>
        /// 回收一个引用对象
        /// </summary>
        /// <param name="obj"></param>
        public static void Free(IReference obj)
        {
            if (Application.isPlaying is false)
            {
                obj?.Release();
                return;
            }

            if (obj is null)
            {
                return;
            }

            GetOrCreareReferenceHandle(obj.GetType()).Release(obj);
        }
    }
}