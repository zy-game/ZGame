using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;
using ZGame.VFS;

namespace ZGame
{
    /// <summary>
    /// 游戏引用对象
    /// </summary>
    public interface IReference : IDisposable
    {
        /// <summary>
        /// 释放引用对象
        /// </summary>
        void IDisposable.Dispose()
        {
            if (this is ResObject resObject)
            {
                ResObject.Unload(resObject);
                return;
            }

            if (this is ResPackage resPackage)
            {
                ResPackage.Unload(resPackage);
            }

            RefPooled.Release(this);
        }

        /// <summary>
        /// 回收引用对象
        /// </summary>
        void Release();
    }

    /// <summary>
    /// 引用工厂
    /// </summary>
    public class RefPooled
    {
        /// <summary>
        /// 引用对象句柄
        /// </summary>
        class ReferenceObjectHandle : IDisposable
        {
            private ConcurrentQueue<IReference> _queue;
            public Type owner { get; }

            public int count => _queue.Count;

            public ReferenceObjectHandle(Type owner)
            {
                this.owner = owner;
                _queue = new();
            }

            /// <summary>
            /// 创建引用对象
            /// </summary>
            /// <returns></returns>
            public IReference Spawner(params object[] args)
            {
                if (_queue.TryDequeue(out IReference result))
                {
                    return result;
                }

                if (args is null || args.Length == 0)
                {
                    return Activator.CreateInstance(owner) as IReference;
                }

                return Activator.CreateInstance(owner, args) as IReference;
            }

            /// <summary>
            /// 回收引用对象
            /// </summary>
            /// <param name="referenceObject"></param>
            public void Release(IReference referenceObject)
            {
                if (referenceObject == null)
                {
                    return;
                }

                referenceObject.Release();
                GC.SuppressFinalize(referenceObject);
                _queue.Enqueue(referenceObject);
            }

            /// <summary>
            /// 释放引用对象句柄
            /// </summary>
            public void Dispose()
            {
                _queue.Clear();
            }
        }

        class GameFrameworkFactoryDrawingHandle
        {
            public void OnDrawingProfile()
            {
            }
        }

        private static readonly List<ReferenceObjectHandle> _queue = new();

        /// <summary>
        /// 获取或创建引用对象句柄
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private static ReferenceObjectHandle GetOrCreareReferenceHandle(Type type)
        {
            ReferenceObjectHandle handle = _queue.Find(x => x.owner == type);
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
        public static T Spawner<T>(params object[] args) where T : IReference
        {
            return (T)Spawner(typeof(T));
        }

        /// <summary>
        /// 创建一个引用对象
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static IReference Spawner(Type type, params object[] args)
        {
            if (typeof(IReference).IsAssignableFrom(type) is false)
            {
                throw new ArgumentException("type must be IGameReferenceObject");
            }

            // Debug.Log("Spawner Reference Object:" + type);
            return GetOrCreareReferenceHandle(type).Spawner();
        }

        /// <summary>
        /// 回收一个引用对象
        /// </summary>
        /// <param name="obj"></param>
        public static void Release(IReference obj)
        {
            if (obj is null)
            {
                return;
            }

            // Debug.Log("Release Reference Object:" + obj.GetType() + "  " + obj.GetHashCode());
            GetOrCreareReferenceHandle(obj.GetType()).Release(obj);
        }
    }
}