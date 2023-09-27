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
    public interface IScheduleHandle : ISchedule
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

        /// <summary>
        /// 调度一个携程函数
        /// </summary>
        /// <param name="func"></param>
        /// <returns></returns>
        public static ICommonScheduleHandle Schedule(Func<ICommonScheduleHandle, IEnumerator> func)
        {
            return ICommonScheduleHandle<object>.InternalCommonScheduleHandle.Create(func);
        }

        /// <summary>
        /// 调度一个异步携程
        /// </summary>
        /// <param name="func"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static ICommonScheduleHandle Schedule<T>(Func<ICommonScheduleHandle<T>, IEnumerator> func)
        {
            return ICommonScheduleHandle<T>.InternalCommonScheduleHandle.Create(func);
        }
    }

    /// <summary>
    /// 执行器
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IScheduleHandle<T> : IScheduleHandle, ISchedule<T>
    {
        /// <summary>
        /// 订阅事件
        /// </summary>
        /// <param name="subscriber"></param>
        public void Subscribe(ISubscriber<T> subscriber)
        {
            Subscribe((ISubscriber)subscriber);
        }
    }

    public interface ICommonScheduleHandle : IScheduleHandle
    {
        void SetResult(object result);
        void SetStatus(Status status);
    }

    public interface ICommonScheduleHandle<T> : ICommonScheduleHandle, IScheduleHandle<T>
    {
        class InternalCommonScheduleHandle : ICommonScheduleHandle<T>
        {
            public T result { get; set; }
            public Status status { get; set; }
            private IEnumerator enumerator;
            private ISubscriber subscriber;

            internal static ICommonScheduleHandle<T> Create(Func<ICommonScheduleHandle<T>, IEnumerator> func)
            {
                InternalCommonScheduleHandle internalCommonScheduleHandle = Activator.CreateInstance<InternalCommonScheduleHandle>();
                internalCommonScheduleHandle.enumerator = func(internalCommonScheduleHandle);
                internalCommonScheduleHandle.Execute();
                return internalCommonScheduleHandle;
            }

            public void Dispose()
            {
                enumerator = null;
                status = Status.None;
                subscriber?.Dispose();
                subscriber = null;
                result = default;
            }

            public void Execute(params object[] args)
            {
                if (status is not Status.None)
                {
                    return;
                }

                status = Status.Execute;
                DOExecute().StartCoroutine(OnComplate);
            }

            private IEnumerator DOExecute()
            {
                yield return enumerator;
            }

            private void OnComplate()
            {
                if (subscriber is not null)
                {
                    subscriber.Execute(result);
                }
            }

            public void Subscribe(ISubscriber subscriber)
            {
                if (this.subscriber is null)
                {
                    this.subscriber = subscriber;
                    return;
                }

                this.subscriber.Merge(subscriber);
            }

            public void SetResult(object result)
            {
                this.result = (T)result;
            }

            public void SetStatus(Status status)
            {
                this.status = status;
            }
        }
    }
}