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

        private object[] dataList;

        public object this[int index] => GetData<object>(index);

        public T GetData<T>(int index)
        {
            if (dataList is null || dataList.Length == 0)
            {
                return default;
            }

            if (index < 0 || index >= dataList.Length)
            {
                Engine.Console.Error(GameEngineException.Create(new IndexOutOfRangeException()));
            }

            return (T)dataList[index];
        }


        public virtual void Release()
        {
            isFree = false;
            dataList = Array.Empty<object>();
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
        public static IExecute Execute(params object[] paramsList)
        {
            T eventArgs = Engine.Class.Loader<T>();
            eventArgs.dataList = paramsList;
            ISubscribe[] subscribes = SubscribeManager.instance.GetSubscribes<T>();
            GameEventExecuteHandle defaultGameEventExecuteHandle = Engine.Class.Loader<GameEventExecuteHandle>();
            defaultGameEventExecuteHandle.Execute(eventArgs, subscribes);
            return defaultGameEventExecuteHandle;
        }

        /// <summary>
        /// 订阅事件
        /// </summary>
        /// <param name="callback">事件回调</param>
        /// <returns>取消事件订阅令牌</returns>
        public static void Subscribe(Action<T> callback)
        {
            SubscribeMethodHandle<T> internalGameEventSubscribeMethod = SubscribeMethodHandle<T>.Create(callback);
            Subscribe(internalGameEventSubscribeMethod);
        }


        /// <summary>
        /// 订阅事件
        /// </summary>
        /// <param name="subscribe">事件订阅器</param>
        /// <returns>取消事件订阅令牌</returns>
        public static void Subscribe(ISubscribe<T> subscribe)
        {
            SubscribeManager.instance.Add<T>(subscribe);
        }

        /// <summary>
        /// 取消事件订阅
        /// </summary>
        /// <param name="callback">事件回调</param>
        public static void Unsubscribe(Action<T> callback)
        {
            SubscribeManager.instance.Remove<T>(callback);
        }

        /// <summary>
        /// 取消事件订阅
        /// </summary>
        /// <param name="subscribe">事件订阅器</param>
        public static void Unsubscribe(ISubscribe<T> subscribe)
        {
            SubscribeManager.instance.Remove<T>(subscribe);
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