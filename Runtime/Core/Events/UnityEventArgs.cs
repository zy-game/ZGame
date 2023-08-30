using System;
using System.Collections.Generic;
using UnityEngine;

namespace ZEngine
{
    public enum GameEventType
    {
        None,
        Update,
        FixedUpdate,
        LateUpdate,
        OnDestroy,
        OnApplicationFocus,
        OnApplicationPause,
        OnApplicationQuit,
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

        internal class UnityEventHandle : MonoBehaviour
        {
            private ISubscribeHandle<UnityEventArgs> update;
            private ISubscribeHandle<UnityEventArgs> fixedupdate;
            private ISubscribeHandle<UnityEventArgs> lateupdate;
            private Dictionary<GameEventType, ISubscribeHandle<UnityEventArgs>> map = new Dictionary<GameEventType, ISubscribeHandle<UnityEventArgs>>();

            private static UnityEventHandle _instance;

            public static UnityEventHandle instance
            {
                get
                {
                    if (_instance == null)
                    {
                        _instance = new GameObject("UnityEventHandle").AddComponent<UnityEventHandle>();
                        GameObject.DontDestroyOnLoad(_instance.gameObject);
                    }

                    return _instance;
                }
            }

            public void Subscribe(GameEventType type, ISubscribeHandle<UnityEventArgs> subscribe)
            {
                if (type == GameEventType.Update)
                {
                    if (update == null)
                    {
                        update = subscribe;
                    }
                    else
                    {
                        update.Merge(subscribe);
                    }

                    return;
                }

                if (type == GameEventType.FixedUpdate)
                {
                    if (fixedupdate == null)
                    {
                        fixedupdate = subscribe;
                    }
                    else
                    {
                        fixedupdate.Merge(subscribe);
                    }

                    return;
                }

                if (type == GameEventType.LateUpdate)
                {
                    if (lateupdate == null)
                    {
                        lateupdate = subscribe;
                    }
                    else
                    {
                        lateupdate.Merge(subscribe);
                    }

                    return;
                }

                if (!map.TryGetValue(type, out ISubscribeHandle<UnityEventArgs> handle))
                {
                    map.Add(type, subscribe);
                    return;
                }

                handle.Merge(subscribe);
            }

            public void Unsubscribe(GameEventType type, ISubscribeHandle<UnityEventArgs> subscribe)
            {
                if (type == GameEventType.Update)
                {
                    update.Unmerge(subscribe);
                    return;
                }

                if (type == GameEventType.FixedUpdate)
                {
                    fixedupdate.Unmerge(subscribe);
                    return;
                }

                if (type == GameEventType.LateUpdate)
                {
                    lateupdate.Unmerge(subscribe);
                    return;
                }

                if (!map.TryGetValue(type, out ISubscribeHandle<UnityEventArgs> handle))
                {
                    return;
                }

                handle.Unmerge(subscribe);
            }

            private void Execute(GameEventType eventType, object data = null)
            {
                if (!map.TryGetValue(eventType, out ISubscribeHandle<UnityEventArgs> subscrbe))
                {
                    return;
                }

                Engine.Console.Log("Mono Event", eventType);
                UnityEventArgs args = Engine.Class.Loader<UnityEventArgs>();
                args.gameObject = this.gameObject;
                args.eventType = eventType;
                args.data = data;
                subscrbe.Execute(args);
            }

            private void Update()
            {
                update?.Execute(default);
            }

            private void FixedUpdate()
            {
                fixedupdate?.Execute(default);
            }

            private void LateUpdate()
            {
                lateupdate?.Execute(default);
            }

            private void OnDestroy()
            {
                Execute(GameEventType.OnDestroy);
            }

            private void OnApplicationFocus(bool hasFocus)
            {
                Execute(GameEventType.OnApplicationFocus, hasFocus);
            }

            private void OnApplicationPause(bool pauseStatus)
            {
                Execute(GameEventType.OnApplicationPause, pauseStatus);
            }
        }
    }
}