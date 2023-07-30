using System;

namespace ZEngine
{
    public abstract class GameEventSubscrbe<T> : ISubscribe<T> where T : GameEventArgs<T>
    {
        public abstract void Execute(T args);

        public abstract void Release();

        public void Execute(params object[] args)
        {
            throw new NotImplementedException();
        }
    }

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

        /// <summary>
        /// 执行分发事件
        /// </summary>
        /// <param name="executeCancelToken">执行取消令牌</param>
        /// <param name="paramsList">事件参数</param>
        /// <returns></returns>
        public static IExecuteHandle Execute(params object[] paramsList)
        {
            T eventArgs = Engine.Class.Loader<T>();
            return SubscribeManager.instance.ExecuteGameEvent(eventArgs);
        }

        /// <summary>
        /// 订阅事件
        /// </summary>
        /// <param name="callback">事件回调</param>
        /// <returns>取消事件订阅令牌</returns>
        public static void Subscribe(Action<T> callback)
        {
            InternalGameEventSubscribe<T> internalGameEventSubscribe = Engine.Class.Loader<InternalGameEventSubscribe<T>>();
            internalGameEventSubscribe.callback = callback;
            Subscribe(internalGameEventSubscribe);
        }


        /// <summary>
        /// 订阅事件
        /// </summary>
        /// <param name="subscribe">事件订阅器</param>
        /// <returns>取消事件订阅令牌</returns>
        public static void Subscribe(ISubscribe<T> subscribe)
        {
            SubscribeManager.instance.Add<T>(subscribe.GetHashCode(), subscribe);
        }

        /// <summary>
        /// 取消事件订阅
        /// </summary>
        /// <param name="callback">事件回调</param>
        public static void Unsubscribe(Action<T> callback)
        {
            SubscribeManager.instance.Remove<T>(callback.GetHashCode());
        }

        /// <summary>
        /// 取消事件订阅
        /// </summary>
        /// <param name="subscribe">事件订阅器</param>
        public static void Unsubscribe(ISubscribe<T> subscribe)
        {
            SubscribeManager.instance.Remove<T>(subscribe.GetHashCode());
        }

        /// <summary>
        /// 清理所有同类型的事件订阅
        /// </summary>
        public static void Clear()
        {
            SubscribeManager.instance.Clear<T>();
        }
    }
}