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
        OnBecameInvisible,
        OnBecameVisible,
        OnCollisionEnter,
        OnCollisionEnter2D,
        OnCollisionExit,
        OnCollisionExit2D,
        OnCollisionStay,
        OnCollisionStay2D,
        OnControllerColliderHit,
        OnDrawGizmos,
        OnDrawGizmosSelected,
        OnTriggerEnter,
        OnTriggerEnter2D,
        OnTriggerExit,
        OnTriggerExit2D,
        OnTriggerStay,
        OnTriggerStay2D,
        OnValidate,
        OnWillRenderObject,
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

        private static UnityEventHandle unityEventHandle;

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
            Subscribe(eventType, GameEventSubscrbe<UnityEventArgs>.Create(eventType, subscribe), gameObject);
        }

        /// <summary>
        /// 订阅引擎事件
        /// </summary>
        /// <param name="eventType">事件类型</param>
        /// <param name="subscribe">订阅器</param>
        /// <param name="gameObject">目标对象</param>
        public static void Subscribe(GameEventType eventType, GameEventSubscrbe<UnityEventArgs> subscribe, GameObject gameObject = null)
        {
            if (unityEventHandle == null)
            {
                unityEventHandle = new GameObject("UnityEventListener").AddComponent<UnityEventHandle>();
                GameObject.DontDestroyOnLoad(unityEventHandle.gameObject);
            }

            if (gameObject == null)
            {
                unityEventHandle.Subscribe(eventType, subscribe);
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
            Unsubscribe(eventType, GameEventSubscrbe<UnityEventArgs>.Create(eventType, subscribe), gameObject);
        }

        /// <summary>
        /// 取消订阅的引擎事件
        /// </summary>
        /// <param name="eventType">事件类型</param>
        /// <param name="subscribe">订阅器</param>
        /// <param name="gameObject">目标对象</param>
        public static void Unsubscribe(GameEventType eventType, GameEventSubscrbe<UnityEventArgs> subscribe, GameObject gameObject = null)
        {
            if (unityEventHandle == null)
            {
                unityEventHandle = new GameObject("UnityEventListener").AddComponent<UnityEventHandle>();
                GameObject.DontDestroyOnLoad(unityEventHandle.gameObject);
            }

            if (gameObject == null)
            {
                unityEventHandle.Unsubscribe(eventType, subscribe);
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