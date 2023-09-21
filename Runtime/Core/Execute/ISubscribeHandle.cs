using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace ZEngine
{
    /// <summary>
    /// 订阅器
    /// </summary>
    public interface ISubscribeHandle : IDisposable
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

    public interface ISubscribeHandle<T> : ISubscribeHandle
    {
        /// <summary>
        /// 指定订阅
        /// </summary>
        /// <param name="args"></param>
        void Execute(T args);

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
            protected List<Action<T>> methods;

            public void Execute(object args)
            {
                Execute((T)args);
            }

            public void Execute(T args)
            {
                for (int i = 0; i < methods.Count; i++)
                {
                    methods[i].Invoke(args);
                }

                if (args is IDisposable reference)
                {
                    reference.Dispose();
                }
            }

            public override bool Equals(object obj)
            {
                if (obj is InternalGameSubscribeHandle subscribe)
                {
                    for (int i = 0; i < subscribe.methods.Count; i++)
                    {
                        if (this.methods.Contains(subscribe.methods[i]) is false)
                        {
                            return false;
                        }
                    }
                }

                return methods.Equals(obj);
            }

            public void Merge(ISubscribeHandle subscribe)
            {
                this.methods.AddRange(((InternalGameSubscribeHandle)subscribe).methods);
                subscribe.Dispose();
            }

            public void Unmerge(ISubscribeHandle subscribe)
            {
                InternalGameSubscribeHandle internalGameSubscribeHandle = (InternalGameSubscribeHandle)subscribe;
                for (int i = 0; i < internalGameSubscribeHandle.methods.Count; i++)
                {
                    this.methods.Remove(internalGameSubscribeHandle.methods[i]);
                }

                internalGameSubscribeHandle.Dispose();
            }

            public void Dispose()
            {
                methods.Clear();
                GC.SuppressFinalize(this);
            }

            internal static InternalGameSubscribeHandle Create(Action<T> callback)
            {
                InternalGameSubscribeHandle internalGameSubscribeHandle = Activator.CreateInstance<InternalGameSubscribeHandle>();
                internalGameSubscribeHandle.methods = new List<Action<T>>() { callback };
                return internalGameSubscribeHandle;
            }
        }
    }
}