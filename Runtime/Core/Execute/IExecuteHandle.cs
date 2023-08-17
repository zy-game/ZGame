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

        /// <summary>
        /// 取消订阅
        /// </summary>
        /// <param name="subscribe"></param>
        void Unsubscribe(ISubscribeHandle subscribe);

        /// <summary>
        /// 取消订阅
        /// </summary>
        /// <param name="callback"></param>
        void Unsubscribe(Action callback)
        {
            this.Unsubscribe(ISubscribeHandle.Create(callback));
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

        /// <summary>
        /// 取消订阅
        /// </summary>
        /// <param name="subscribe"></param>
        void Unsubscribe(ISubscribeHandle<T> subscribe)
        {
            Unsubscribe((ISubscribeHandle)subscribe);
        }

        /// <summary>
        /// 取消订阅
        /// </summary>
        /// <param name="callback"></param>
        void Unsubscribe(Action<T> callback)
        {
            Unsubscribe(ISubscribeHandle<T>.Create(callback));
        }
    }

    public abstract class ExecuteHandle : IExecuteHandle
    {
        protected List<ISubscribeHandle> subscribes;
        protected ISubscribeHandle<float> progresSubsceibe;
        public Status status { get; protected set; }

        public ExecuteHandle()
        {
            subscribes = new List<ISubscribeHandle>();
        }

        public abstract void Execute(params object[] paramsList);

        public void Subscribe(ISubscribeHandle subscribe)
        {
            subscribes.Add(subscribe);
        }

        public void OnPorgressChange(ISubscribeHandle<float> subscribe)
        {
            progresSubsceibe = subscribe;
        }

        public void Unsubscribe(ISubscribeHandle subscribe)
        {
            subscribes.Remove(subscribe);
        }

        protected void OnProgress(float progress)
        {
            progresSubsceibe?.Execute(progress);
        }

        public virtual void Release()
        {
            if (subscribes.Count() > 0)
            {
                subscribes.ForEach(Engine.Class.Release);
                subscribes.Clear();
            }

            Engine.Class.Release(progresSubsceibe);
            progresSubsceibe = null;
            status = Status.None;
            GC.SuppressFinalize(this);
        }

        protected virtual void OnComplete()
        {
            for (int i = 0; i < subscribes.Count; i++)
            {
                subscribes[i].Execute(this);
            }

            WaitFor.WaitFormFrameEnd(() => { Engine.Class.Release(this); });
        }
    }
}