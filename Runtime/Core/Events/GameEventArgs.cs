using System;
using System.Collections.Generic;

namespace ZEngine
{
    /// <summary>
    /// 游戏事件参数
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class GameEventArgs<T> : IReference where T : GameEventArgs<T>
    {
        private bool isFree = false;

        public virtual void Release()
        {
            isFree = false;
        }

        /// <summary>
        /// 释放事件，调用此函数后，后续事件订阅器将不会被触发
        /// </summary>
        public void Free()
        {
            isFree = true;
        }

        /// <summary>
        /// 事件是否被释放
        /// </summary>
        /// <returns></returns>
        public bool HasFree()
        {
            return isFree;
        }


        private static List<GameEventSubscrbe<T>> subscribes = new List<GameEventSubscrbe<T>>();

        /// <summary>
        /// 执行分发事件
        /// </summary>
        /// <param name="executeCancelToken">执行取消令牌</param>
        /// <param name="paramsList">事件参数</param>
        /// <returns></returns>
        public static void Execute(T args)
        {
            Engine.Console.Log("subscribe count:" + subscribes.Count);
            for (int i = subscribes.Count - 1; i >= 0; i--)
            {
                subscribes[i].Execute(args);
            }
        }

        /// <summary>
        /// 订阅事件
        /// </summary>
        /// <param name="callback">事件回调</param>
        /// <returns>取消事件订阅令牌</returns>
        public static void Subscribe(Action<T> callback)
        {
            Subscribe(GameEventSubscrbe<T>.Create(callback));
        }


        /// <summary>
        /// 订阅事件
        /// </summary>
        /// <param name="subscribe">事件订阅器</param>
        /// <returns>取消事件订阅令牌</returns>
        public static void Subscribe(GameEventSubscrbe<T> subscribe)
        {
            subscribes.Add(subscribe);
        }

        public static void Subscribe<T2>() where T2 : GameEventSubscrbe<T>
        {
            Subscribe(Engine.Class.Loader<T2>());
        }

        /// <summary>
        /// 取消事件订阅
        /// </summary>
        /// <param name="callback">事件回调</param>
        public static void Unsubscribe(Action<T> callback)
        {
            GameEventSubscrbe<T> subscribeExecuteHandle = subscribes.Find(x => x.Equals(callback));
            Unsubscribe(subscribeExecuteHandle);
        }

        /// <summary>
        /// 取消事件订阅
        /// </summary>
        /// <param name="subscribe">事件订阅器</param>
        public static void Unsubscribe(GameEventSubscrbe<T> subscribe)
        {
            if (subscribe is null)
            {
                return;
            }

            subscribes.Remove(subscribe);
        }

        public static void Unsubscribe<T2>() where T2 : GameEventSubscrbe<T>
        {
            GameEventSubscrbe<T> subscribeExecuteHandle = subscribes.Find(x => x.GetType() == typeof(T2));
            Unsubscribe(subscribeExecuteHandle);
        }

        /// <summary>
        /// 清理所有同类型的事件订阅
        /// </summary>
        public static void Clear()
        {
            subscribes.Clear();
        }
    }
}