using System;
using UnityEngine;

namespace ZEngine
{
    public enum GameEventType
    {
        None,
        Update,
        FixedUpdate,
        LateUpdate,
        OnEnable,
        OnDisable,
        OnDestroy,
        OnApplicationFocus,
        OnApplicationPause,
        OnApplicationQuit
    }

    /// <summary>
    /// Unity引擎事件
    /// </summary>
    public sealed class UnityEventArgs : GameEventArgs<UnityEventArgs>
    {
        public GameObject gameObject;
        public GameEventType eventType;
        public object data;

        public override void Release()
        {
            gameObject = null;
            eventType = GameEventType.None;
            data = null;
            GC.SuppressFinalize(this);
            base.Release();
        }

        /// <summary>
        /// 订阅引擎事件
        /// </summary>
        /// <param name="eventType">事件类型</param>
        /// <param name="subscribe">订阅器</param>
        /// <param name="gameObject">目标对象</param>
        public static void Subscribe(GameEventType eventType, Action<UnityEventArgs> subscribe, GameObject gameObject = null)
        {
            Subscribe(eventType, ISubscribeHandle<UnityEventArgs>.Create(subscribe), gameObject);
        }

        /// <summary>
        /// 订阅引擎事件
        /// </summary>
        /// <param name="eventType">事件类型</param>
        /// <param name="subscribe">订阅器</param>
        /// <param name="gameObject">目标对象</param>
        public static void Subscribe(GameEventType eventType, ISubscribeHandle<UnityEventArgs> subscribe, GameObject gameObject = null)
        {
            if (gameObject == null)
            {
                UnityEventHandle.instance.Subscribe(eventType, subscribe);
                return;
            }

            UnityEventHandle handle = gameObject.GetComponent<UnityEventHandle>();
            if (handle == null)
            {
                handle = gameObject.AddComponent<UnityEventHandle>();
            }

            handle.Subscribe(eventType, subscribe);
        }

        /// <summary>
        /// 取消订阅的引擎事件
        /// </summary>
        /// <param name="eventType">事件类型</param>
        /// <param name="subscribe">订阅器</param>
        /// <param name="gameObject">目标对象</param>
        public static void Unsubscribe(GameEventType eventType, Action<UnityEventArgs> subscribe, GameObject gameObject = null)
        {
            Unsubscribe(eventType, ISubscribeHandle<UnityEventArgs>.Create(subscribe), gameObject);
        }

        /// <summary>
        /// 取消订阅的引擎事件
        /// </summary>
        /// <param name="eventType">事件类型</param>
        /// <param name="subscribe">订阅器</param>
        /// <param name="gameObject">目标对象</param>
        public static void Unsubscribe(GameEventType eventType, ISubscribeHandle<UnityEventArgs> subscribe, GameObject gameObject = null)
        {
            if (gameObject == null)
            {
                UnityEventHandle.instance.Unsubscribe(eventType, subscribe);
                return;
            }

            UnityEventHandle handle = gameObject.GetComponent<UnityEventHandle>();
            if (handle == null)
            {
                return;
            }

            handle.Unsubscribe(eventType, subscribe);
        }
    }
}