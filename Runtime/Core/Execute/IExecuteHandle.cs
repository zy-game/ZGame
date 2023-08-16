using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ZEngine.VFS;

namespace ZEngine
{
    /// <summary>
    /// 异步执行器
    /// </summary>
    public interface IExecuteHandle : IExecute
    {
        /// <summary>
        /// 执行器状态
        /// </summary>
        Status status { get; }


        /// <summary>
        /// 订阅执行器完成回调
        /// </summary>
        /// <param name="subscribe">订阅器</param>
        void Subscribe(ISubscribeHandle subscribe);

        /// <summary>
        /// 订阅执行器进度
        /// </summary>
        /// <param name="subscribe"></param>
        void OnPorgressChange(ISubscribeHandle<float> subscribe);

        /// <summary>
        /// 订阅执行完成回调
        /// </summary>
        /// <param name="callback">回调对象</param>
        void Subscribe(Action callback)
        {
            this.Subscribe(ISubscribeHandle.Create(callback));
        }
    }

    /// <summary>
    /// 异步执行器
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IExecuteHandle<T> : IExecuteHandle, IExecute
    {
        /// <summary>
        /// 订阅执行器完成回调
        /// </summary>
        /// <param name="subscribe">订阅器</param>
        void Subscribe(ISubscribeHandle<T> subscribe)
        {
            Subscribe((ISubscribeHandle)subscribe);
        }

        /// <summary>
        /// 订阅执行器完成回调
        /// </summary>
        /// <param name="callback">订阅对象</param>
        void Subscribe(Action<T> callback)
        {
            this.Subscribe(ISubscribeHandle<T>.Create(callback));
        }
    }

    public abstract class ExecuteHandle : IExecuteHandle
    {
        protected Queue<ISubscribeHandle> handles;
        protected ISubscribeHandle<float> progresSubsceibe;
        public Status status { get; protected set; }

        public ExecuteHandle()
        {
            handles = new Queue<ISubscribeHandle>();
        }

        public abstract void Execute(params object[] paramsList);

        public void Subscribe(ISubscribeHandle subscribe)
        {
            handles.Enqueue(subscribe);
        }

        public void OnPorgressChange(ISubscribeHandle<float> subscribe)
        {
            progresSubsceibe = subscribe;
        }

        protected void OnProgress(float progress)
        {
            progresSubsceibe?.Execute(progress);
        }

        public virtual void Release()
        {
            if (handles.Count() > 0)
            {
                while (handles.TryDequeue(out ISubscribeHandle subscribe))
                {
                    Engine.Class.Release(subscribe);
                }
            }

            Engine.Class.Release(progresSubsceibe);
            progresSubsceibe = null;
            status = Status.None;
            GC.SuppressFinalize(this);
        }

        protected virtual void OnComplete()
        {
            while (handles.TryDequeue(out ISubscribeHandle subscribe))
            {
                subscribe.Execute(this);
            }

            WaitFor.WaitFormFrameEnd(() => { Engine.Class.Release(this); });
        }
    }
}