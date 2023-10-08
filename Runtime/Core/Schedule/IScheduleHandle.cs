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
        public static IScheduleHandle Schedule(Func<IScheduleHandle, IEnumerator> func)
        {
            return IScheduleHandle<object>.InternalScheduleHandle.Create(func);
        }

        /// <summary>
        /// 调度一个异步携程
        /// </summary>
        /// <param name="func"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IScheduleHandle<T> Schedule<T>(Func<IScheduleHandle<T>, IEnumerator> func)
        {
            return IScheduleHandle<T>.InternalScheduleHandle.Create(func);
        }

        /// <summary>
        /// 调度一个异步携程
        /// </summary>
        /// <param name="func"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IScheduleHandle<T> Schedule<T>(IScheduleToken<T> token)
        {
            return IScheduleHandle<T>.InternalScheduleHandle.Create(token);
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

        class InternalScheduleHandle : IScheduleHandle<T>
        {
            public T result { get; set; }
            public Status status { get; set; }
            private IEnumerator enumerator;
            private IScheduleToken<T> token;
            private ISubscriber subscriber;

            internal static IScheduleHandle<T> Create(Func<IScheduleHandle<T>, IEnumerator> func)
            {
                InternalScheduleHandle internalCommonScheduleHandle = Activator.CreateInstance<InternalScheduleHandle>();
                internalCommonScheduleHandle.enumerator = func(internalCommonScheduleHandle);
                internalCommonScheduleHandle.Execute();
                return internalCommonScheduleHandle;
            }

            internal static IScheduleHandle<T> Create(IScheduleToken<T> token)
            {
                InternalScheduleHandle internalCommonScheduleHandle = Activator.CreateInstance<InternalScheduleHandle>();
                internalCommonScheduleHandle.token = token;
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
                token?.Dispose();
                token = null;
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
                if (token is not null)
                {
                    yield return WaitFor.Create(() => token.isComplate);
                }
                else
                {
                    yield return enumerator;
                }
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