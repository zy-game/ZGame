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
    }

    public abstract class ExecuteHandle : IExecuteHandle
    {
        protected ISubscribeHandle subscribes;
        protected ISubscribeHandle<float> progresSubsceibe;
        public Status status { get; protected set; }


        public abstract void Execute(params object[] paramsList);

        public void Subscribe(ISubscribeHandle subscribe)
        {
            if (subscribes is null)
            {
                subscribes = subscribe;
                return;
            }

            subscribes.Merge(subscribe);
            Engine.Class.Release(subscribe);
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
            Engine.Class.Release(subscribes);
            Engine.Class.Release(progresSubsceibe);
            progresSubsceibe = null;
            status = Status.None;
            GC.SuppressFinalize(this);
        }

        protected virtual void OnComplete()
        {
            subscribes?.Execute(this);
            WaitFor.WaitFormFrameEnd(() => { Engine.Class.Release(this); });
        }
    }
}