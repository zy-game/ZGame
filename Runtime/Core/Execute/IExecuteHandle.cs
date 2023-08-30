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
        void Subscribe(ISubscribeHandle subscribe);
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
        private Coroutine coroutine;
        private ISubscribeHandle subscribes;
        private IProgressSubscribeHandle progressSubscribeHandle;
        public Status status { get; protected set; }
        protected abstract IEnumerator ExecuteCoroutine();

        public void Execute()
        {
            status = Status.Execute;
            coroutine = ExecuteCoroutine().StartCoroutine(OnComplete);
        }

        private void OnComplete()
        {
            subscribes?.Execute(this);
            WaitFor.WaitFormFrameEnd(() => { Engine.Class.Release(this); });
        }

        public void Subscribe(ISubscribeHandle subscribe)
        {
            if (subscribe is IProgressSubscribeHandle progressSubscribeHandle)
            {
                if (this.progressSubscribeHandle is null)
                {
                    this.progressSubscribeHandle = progressSubscribeHandle;
                    return;
                }

                this.progressSubscribeHandle.Merge(progressSubscribeHandle);
                return;
            }

            if (this.subscribes is null)
            {
                this.subscribes = subscribe;
                return;
            }

            this.subscribes.Merge(subscribe);
        }

        protected void OnProgress(float progress)
        {
            progressSubscribeHandle?.Execute(progress);
        }

        public virtual void Release()
        {
            coroutine.StopCoroutine();
            Engine.Class.Release(subscribes);
            subscribes = null;
            Engine.Class.Release(progressSubscribeHandle);
            progressSubscribeHandle = null;
            status = Status.None;
            GC.SuppressFinalize(this);
        }
    }
}