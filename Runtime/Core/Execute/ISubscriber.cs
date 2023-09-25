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

        /// <summary>
        /// 创建一个订阅器
        /// </summary>
        /// <param name="call">订阅回调</param>
        /// <param name="gameObject">订阅对象</param>
        /// <returns></returns>
        public static ISubscriber Create(Action call, GameObject gameObject)
        {
            ISubscriber subscriber = ISubscriber<object>.InternalGameSubscriber.Create(call);
            gameObject.TryGetComponent<UnityBehaviour>().OnDestroyGameObject(() => subscriber.Execute(null));
            return subscriber;
        }


        private static Dictionary<Type, ISubscriber> subscribers = new Dictionary<Type, ISubscriber>();

        /// <summary>
        /// 执行订阅
        /// </summary>
        /// <param name="args"></param>
        public static void Execute(GameEventArgs args)
        {
            if (subscribers.TryGetValue(args.GetType(), out ISubscriber subscriber) is false)
            {
                return;
            }

            subscriber.Execute(args);
        }

        /// <summary>
        /// 订阅消息
        /// </summary>
        /// <param name="callback">订阅回调</param>
        /// <typeparam name="T">事件类型</typeparam>
        public static void Subscribe<T>(Action<T> callback) where T : GameEventArgs
        {
            Type type = typeof(T);
            if (subscribers.TryGetValue(type, out ISubscriber subscriber) is false)
            {
                subscribers.Add(type, subscriber = ISubscriber.Create<T>(callback));
                return;
            }

            subscriber.Subscribe(ISubscriber.Create<T>(callback));
        }

        /// <summary>
        /// 订阅消息
        /// </summary>
        /// <param name="callback">订阅回调</param>
        /// <typeparam name="T">事件类型</typeparam>
        public static void Subscribe<T>(ISubscriber<T> callback) where T : GameEventArgs
        {
            Type type = typeof(T);
            if (subscribers.TryGetValue(type, out ISubscriber subscriber) is false)
            {
                subscribers.Add(type, subscriber = callback);
                return;
            }

            subscriber.Subscribe(callback);
        }

        /// <summary>
        /// 订阅消息
        /// </summary>
        /// <param name="eventType">事件类型</param>
        /// <param name="callback">订阅回调</param>
        public static void Subscribe(Type eventType, Action<GameEventArgs> callback)
        {
            if (subscribers.TryGetValue(eventType, out ISubscriber subscriber) is false)
            {
                subscribers.Add(eventType, subscriber = ISubscriber.Create<GameEventArgs>(callback));
                return;
            }

            subscriber.Subscribe(ISubscriber.Create<GameEventArgs>(callback));
        }

        /// <summary>
        /// 订阅消息
        /// </summary>
        /// <param name="eventType">事件类型</param>
        /// <param name="callback">订阅回调</param>
        public static void Subscribe(Type eventType, ISubscriber<GameEventArgs> callback)
        {
            if (subscribers.TryGetValue(eventType, out ISubscriber subscriber) is false)
            {
                subscribers.Add(eventType, subscriber = callback);
                return;
            }

            subscriber.Subscribe(callback);
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