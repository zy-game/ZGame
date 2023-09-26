using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace ZEngine
{
    /// <summary>
    /// 订阅器
    /// </summary>
    public interface ISubscriber : IDisposable
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
        void Subscribe(ISubscriber subscribe);

        /// <summary>
        /// 取消合并
        /// </summary>
        /// <param name="subscribe"></param>
        void Unsubscribe(ISubscriber subscribe);

        /// <summary>
        /// 创建一个订阅器
        /// </summary>
        /// <param name="action">回调函数</param>
        /// <returns>订阅器</returns>
        public static ISubscriber Create(Action action = null)
        {
            return ISubscriber<object>.InternalGameSubscriber.Create(action);
        }

        /// <summary>
        /// 创建一个订阅器
        /// </summary>
        /// <param name="callback">回调函数</param>
        /// <typeparam name="T">订阅类型</typeparam>
        /// <returns>订阅器</returns>
        public static ISubscriber<T> Create<T>(Action<T> callback = null)
        {
            return ISubscriber<T>.InternalGameSubscriber.Create(callback);
        }

    }

    public interface ISubscriber<T> : ISubscriber
    {
        /// <summary>
        /// 指定订阅
        /// </summary>
        /// <param name="args"></param>
        void Execute(T args);


        class InternalGameSubscriber : ISubscriber<T>
        {
            private Dictionary<int, Action<T>> map = new Dictionary<int, Action<T>>();

            public void Execute(object args)
            {
                if (args is not T)
                {
                    Engine.Console.Error("未知类型", args.GetType(), typeof(T));
                    return;
                }

                Execute((T)args);
            }

            public void Execute(T args)
            {
                Action<T>[] list = map.Values.ToArray();
                foreach (var VARIABLE in list)
                {
                    VARIABLE(args);
                }

                if (args is IDisposable reference)
                {
                    reference.Dispose();
                }
            }

            public void Subscribe(ISubscriber subscribe)
            {
                if (subscribe is not InternalGameSubscriber)
                {
                    Engine.Console.Error("未知类型", subscribe.GetType(), this.GetType());
                    return;
                }

                InternalGameSubscriber internalGameSubscriber = (InternalGameSubscriber)subscribe;
                if (internalGameSubscriber == null)
                {
                    return;
                }

                foreach (var VARIABLE in internalGameSubscriber.map)
                {
                    if (this.map.ContainsKey(VARIABLE.Key))
                    {
                        Engine.Console.Log("存在重复的订阅回调：" + VARIABLE.Value);
                        continue;
                    }

                    this.map.Add(VARIABLE.Key, VARIABLE.Value);
                }

                subscribe.Dispose();
            }

            public void Unsubscribe(ISubscriber subscribe)
            {
                if (subscribe is not InternalGameSubscriber)
                {
                    Engine.Console.Error("未知类型", subscribe.GetType(), this.GetType());
                    return;
                }

                InternalGameSubscriber internalGameSubscriber = (InternalGameSubscriber)subscribe;
                if (internalGameSubscriber == null)
                {
                    return;
                }

                foreach (var VARIABLE in internalGameSubscriber.map.Keys)
                {
                    this.map.Remove(VARIABLE);
                }

                internalGameSubscriber.Dispose();
            }

            public void Dispose()
            {
                map.Clear();
                GC.SuppressFinalize(this);
            }

            public override bool Equals(object obj)
            {
                if (obj is Action<T> action)
                {
                    return this.map.ContainsKey(action.GetHashCode());
                }

                if (obj is InternalGameSubscriber subscriber)
                {
                    foreach (var VARIABLE in subscriber.map)
                    {
                        if (this.map.ContainsKey(VARIABLE.Key) is false)
                        {
                            return false;
                        }
                    }
                }


                return this.Equals(obj);
            }

            internal static InternalGameSubscriber Create(Action callback)
            {
                InternalGameSubscriber internalGameSubscriber = Activator.CreateInstance<InternalGameSubscriber>();
                internalGameSubscriber.map = new Dictionary<int, Action<T>>();
                if (callback is not null)
                {
                    internalGameSubscriber.map.Add(callback.GetHashCode(), _ => callback());
                }

                return internalGameSubscriber;
            }

            internal static InternalGameSubscriber Create(Action<T> callback)
            {
                InternalGameSubscriber internalGameSubscriber = Activator.CreateInstance<InternalGameSubscriber>();
                internalGameSubscriber.map = new Dictionary<int, Action<T>>();
                if (callback is not null)
                {
                    internalGameSubscriber.map.Add(callback.GetHashCode(), callback);
                }

                return internalGameSubscriber;
            }
        }
    }
}