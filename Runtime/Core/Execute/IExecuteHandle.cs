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
        /// 订阅事件
        /// </summary>
        /// <param name="subscriber"></param>
        void Subscribe(ISubscriber subscriber);

        public static GameExecuteHandle Create(IEnumerator enumerator = null)
        {
            return InternalExecuteHandle<IExecuteHandle>.Create(enumerator);
        }
    }

    /// <summary>
    /// 执行器
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IExecuteHandle<T> : IExecuteHandle
    {
        /// <summary>
        /// 订阅事件
        /// </summary>
        /// <param name="subscriber"></param>
        void Subscribe(ISubscriber<T> subscriber);

        public static GameExecuteHandle<T> Create(IEnumerator enumerator = null)
        {
            return InternalExecuteHandle<T>.Create(enumerator);
        }
    }

    class InternalExecuteHandle<T> : GameExecuteHandle<T>
    {
        public static InternalExecuteHandle<T> Create(IEnumerator enumerator = null)
        {
            InternalExecuteHandle<T> internalExecuteHandle = new InternalExecuteHandle<T>();
            internalExecuteHandle.SetMethod(enumerator);
            internalExecuteHandle.Execute();
            return internalExecuteHandle;
        }
    }

    public abstract class GameExecuteHandle : IExecuteHandle
    {
        private IEnumerator enumerator;
        private ISubscriber subscribes;
        public Status status { get; set; }


        protected virtual IEnumerator DOExecute()
        {
            if (enumerator is null)
            {
                yield break;
            }

            yield return enumerator;
        }

        public void SetMethod(IEnumerator enumerator)
        {
            this.enumerator = enumerator;
        }

        public void Subscribe(ISubscriber subscribe)
        {
            if (subscribes is not null)
            {
                this.subscribes.Subscribe(subscribe);
                return;
            }

            this.subscribes = subscribe;
        }

        public void Execute()
        {
            status = Status.Execute;
            DOExecute().StartCoroutine(OnComplete);
        }

        private void OnComplete()
        {
            subscribes?.Execute(this);
            WaitFor.WaitFormFrameEnd(this.Dispose);
        }


        public virtual void Dispose()
        {
            subscribes?.Dispose();
            subscribes = null;
            status = Status.None;
            GC.SuppressFinalize(this);
        }
    }

    public abstract class GameExecuteHandle<T> : GameExecuteHandle, IExecuteHandle<T>
    {
        public void Subscribe(ISubscriber<T> subscriber)
        {
            this.Subscribe((ISubscriber)subscriber);
        }
    }
}