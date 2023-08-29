using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;

namespace ZEngine
{
    /// <summary>
    /// 订阅器
    /// </summary>
    public interface ISubscribeHandle : IReference
    {
        /// <summary>
        /// 执行订阅函数
        /// </summary>
        /// <param name="args"></param>
        void Execute(object args);

        /// <summary>
        /// 合并订阅
        /// </summary>
        /// <param name="subscribe"></param>
        void Merge(ISubscribeHandle subscribe);

        /// <summary>
        /// 取消合并
        /// </summary>
        /// <param name="subscribe"></param>
        void Unmerge(ISubscribeHandle subscribe);

        /// <summary>
        /// 创建一个订阅器
        /// </summary>
        /// <param name="action">回调函数</param>
        /// <returns>订阅器</returns>
        public static ISubscribeHandle Create(Action action)
        {
            return ISubscribeHandle<object>.InternalGameSubscribeHandle.Create(args => action());
        }
    }

    /// <summary>
    /// 订阅器
    /// </summary>
    public interface IProgressSubscribeHandle : ISubscribeHandle<float>
    {
        /// <summary>
        /// 创建一个进度订阅器
        /// </summary>
        /// <param name="callback">回调函数</param>
        /// <returns>订阅器</returns>
        public static IProgressSubscribeHandle Create(Action<float> callback)
        {
            return InternalProgressSubscribeHandle.Create(callback);
        }

        class InternalProgressSubscribeHandle : IProgressSubscribeHandle
        {
            private Action<float> method;

            public void Execute(object args)
            {
                Execute((float)args);
            }

            public void Execute(float args)
            {
                method?.Invoke(args);
            }

            public void Merge(ISubscribeHandle subscribe)
            {
                if (subscribe is InternalProgressSubscribeHandle internalProgressSubscribeHandle)
                {
                    this.method += internalProgressSubscribeHandle.method;
                }
            }

            public void Unmerge(ISubscribeHandle subscribe)
            {
                if (subscribe is InternalProgressSubscribeHandle internalProgressSubscribeHandle)
                {
                    this.method -= internalProgressSubscribeHandle.method;
                }
            }

            public void Release()
            {
                method = null;
                GC.SuppressFinalize(this);
            }

            public static InternalProgressSubscribeHandle Create(Action<float> callback)
            {
                InternalProgressSubscribeHandle internalProgressSubscribeHandle = Engine.Class.Loader<InternalProgressSubscribeHandle>();
                internalProgressSubscribeHandle.method = callback;
                return internalProgressSubscribeHandle;
            }
        }
    }

    public interface ISubscribeHandle<T> : ISubscribeHandle
    {
        /// <summary>
        /// 指定订阅
        /// </summary>
        /// <param name="args"></param>
        void Execute(T args);

        /// <summary>
        /// 合并订阅
        /// </summary>
        /// <param name="subscribe"></param>
        void Merge(ISubscribeHandle<T> subscribe)
        {
            Merge((ISubscribeHandle)subscribe);
        }

        /// <summary>
        /// 取消合并
        /// </summary>
        /// <param name="subscribe"></param>
        void Unmerge(ISubscribeHandle<T> subscribe)
        {
            Unmerge((ISubscribeHandle)subscribe);
        }

        /// <summary>
        /// 创建一个订阅器
        /// </summary>
        /// <param name="callback">回调函数</param>
        /// <typeparam name="T">订阅类型</typeparam>
        /// <returns>订阅器</returns>
        public static ISubscribeHandle<T> Create(Action<T> callback)
        {
            return ISubscribeHandle<T>.InternalGameSubscribeHandle.Create(callback);
        }
        
        

        class InternalGameSubscribeHandle : ISubscribeHandle<T>
        {
            protected Action<T> method;

            public virtual void Execute(object args)
            {
                Execute((T)args);
            }

            public void Execute(T args)
            {
                method?.Invoke(args);
                if (args is IReference reference)
                {
                    Engine.Class.Release(reference);
                }
            }

            public void Merge(ISubscribeHandle subscribe)
            {
                this.method += ((InternalGameSubscribeHandle)subscribe).method;
            }

            public void Unmerge(ISubscribeHandle subscribe)
            {
                this.method -= ((InternalGameSubscribeHandle)subscribe).method;
            }

            public void Release()
            {
                method = null;
                GC.SuppressFinalize(this);
            }

            internal static InternalGameSubscribeHandle Create(Action<T> callback)
            {
                InternalGameSubscribeHandle internalGameSubscribeHandle = Engine.Class.Loader<InternalGameSubscribeHandle>();
                internalGameSubscribeHandle.method = callback;
                return internalGameSubscribeHandle;
            }
        }
    }
}