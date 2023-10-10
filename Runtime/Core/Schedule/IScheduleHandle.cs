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
            => IScheduleHandle<object>.InternalScheduleHandle.Create(func);

        /// <summary>
        /// 调度一个异步携程
        /// </summary>
        /// <param name="func"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IScheduleHandle<T> Schedule<T>(Func<IScheduleHandle<T>, IEnumerator> func)
            => IScheduleHandle<T>.InternalScheduleHandle.Create(func);

        /// <summary>
        /// 调度一个异步携程
        /// </summary>
        /// <param name="func"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IScheduleHandle Schedule(IScheduleHandleToken token)
            => IScheduleHandle<object>.InternalScheduleHandleWithToken.Create(token);

        /// <summary>
        /// 调度一个异步携程
        /// </summary>
        /// <param name="func"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IScheduleHandle<T> Schedule<T>(IScheduleHandleToken<T> token)
            => IScheduleHandle<T>.InternalScheduleHandleWithToken.Create(token);

        public static IScheduleHandle Complate()
            => IScheduleHandle<object>.InternalScheduleHandle.OnNormal(default, Status.Success);

        public static IScheduleHandle<T> Complate<T>(T result)
            => IScheduleHandle<T>.InternalScheduleHandle.OnNormal(result, Status.Success);

        public static IScheduleHandle Failur()
            => IScheduleHandle<object>.InternalScheduleHandle.OnNormal(default, Status.Failed);

        public static IScheduleHandle<T> Failur<T>()
            => IScheduleHandle<T>.InternalScheduleHandle.OnNormal(default, Status.Failed);
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

        class InternalScheduleHandleWithToken : IScheduleHandle<T>
        {
            public T result { get; }
            public Status status { get; }
            private IScheduleHandleToken token;

            public void Dispose()
            {
                throw new NotImplementedException();
            }

            public void Execute(params object[] args)
            {
                throw new NotImplementedException();
            }


            public void Subscribe(ISubscriber subscriber)
            {
                throw new NotImplementedException();
            }
        }

        class InternalScheduleHandle : IScheduleHandle<T>
        {
            public T result { get; set; }
            public Status status { get; set; }
            private IEnumerator enumerator;
            private ISubscriber subscriber;

            internal static IScheduleHandle<T> OnNormal(T result, Status status)
            {
                InternalScheduleHandle internalCommonScheduleHandle = Activator.CreateInstance<InternalScheduleHandle>();
                internalCommonScheduleHandle.status = status;
                internalCommonScheduleHandle.result = result;
                return internalCommonScheduleHandle;
            }

            internal static IScheduleHandle<T> Create(Func<IScheduleHandle<T>, IEnumerator> func)
            {
                InternalScheduleHandle internalCommonScheduleHandle = Activator.CreateInstance<InternalScheduleHandle>();
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