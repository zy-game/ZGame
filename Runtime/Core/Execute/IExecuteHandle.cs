using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ZEngine.VFS;

namespace ZEngine
{
    /// <summary>
    /// 执行器
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
        void Subscribe(ISubscriber subscribe);
    }

    /// <summary>
    /// 执行器
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IExecuteHandle<T> : IExecuteHandle, IExecute
    {
    }

    public abstract class AbstractExecuteHandle : IExecuteHandle
    {
        private ISubscriber subscribes;
        public Status status { get; protected set; }
        protected abstract IEnumerator OnExecute();

        public void Execute()
        {
            status = Status.Execute;
            OnExecute().StartCoroutine(OnComplete);
        }

        private void OnComplete()
        {
            subscribes?.Execute(this);
            WaitFor.WaitFormFrameEnd(this.Dispose);
        }

        public void Subscribe(ISubscriber subscribe)
        {
            if (this.subscribes is null)
            {
                this.subscribes = subscribe;
                return;
            }

            this.subscribes.Merge(subscribe);
        }


        public virtual void Dispose()
        {
            subscribes?.Dispose();
            subscribes = null;
            status = Status.None;
            GC.SuppressFinalize(this);
        }
    }
}