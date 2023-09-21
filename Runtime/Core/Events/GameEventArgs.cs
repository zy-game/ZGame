using System;
using System.Collections.Generic;
using UnityEngine;

namespace ZEngine
{
    /// <summary>
    /// 游戏事件参数
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class GameEventArgs<T> : IDisposable where T : GameEventArgs<T>
    {
        private bool isFree = false;

        public virtual void Initialized(params object[] paramsList)
        {
        }

        public virtual void Dispose()
        {
            isFree = false;
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// 释放事件，调用此函数后，后续事件订阅器将不会被触发
        /// </summary>
        public void Free()
        {
            isFree = true;
        }

        static ISubscribeHandle<T> _subscrbe;

        public static void Execute(params object[] paramsList)
        {
            T args = Activator.CreateInstance<T>();
            args.Initialized(paramsList);
            Execute(args);
        }

        /// <summary>
        /// 执行分发事件
        /// </summary>
        /// <param name="executeCancelToken">执行取消令牌</param>
        /// <param name="paramsList">事件参数</param>
        /// <returns></returns>
        public static void Execute(T args)
        {
            if (args.isFree)
            {
                Engine.Console.Log("the event is use");
                return;
            }

            if (_subscrbe is null)
            {
                return;
            }

            _subscrbe.Execute(args);
            args.Dispose();
        }

        /// <summary>
        /// 订阅事件
        /// </summary>
        /// <param name="callback">事件回调</param>
        /// <returns>取消事件订阅令牌</returns>
        public static void Subscribe(Action<T> callback)
        {
            Subscribe(ISubscribeHandle<T>.Create(callback));
        }

        /// <summary>
        /// 取消事件订阅
        /// </summary>
        /// <param name="callback">事件回调</param>
        public static void Unsubscribe(Action<T> callback)
        {
            Unsubscribe(ISubscribeHandle<T>.Create(callback));
        }

        /// <summary>
        /// 订阅事件
        /// </summary>
        /// <param name="subscribe">事件订阅器</param>
        /// <returns>取消事件订阅令牌</returns>
        public static void Subscribe(ISubscribeHandle<T> subscribe)
        {
            if (_subscrbe is not null)
            {
                _subscrbe.Merge(subscribe);
            }
        }

        /// <summary>
        /// 取消事件订阅
        /// </summary>
        /// <param name="subscribe">事件订阅器</param>
        public static void Unsubscribe(ISubscribeHandle<T> subscribe)
        {
            if (subscribe is null)
            {
                return;
            }

            if (_subscrbe is not null)
            {
                _subscrbe.Unmerge(subscribe);
            }
        }

        /// <summary>
        /// 清理所有同类型的事件订阅
        /// </summary>
        public static void ClearSubscribe()
        {
            _subscrbe.Dispose();
            _subscrbe = null;
        }
    }
}