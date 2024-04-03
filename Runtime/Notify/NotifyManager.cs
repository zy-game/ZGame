using System;
using System.Collections.Generic;
using UnityEngine;

namespace ZGame.Notify
{
    public class KeyEventArgs : IGameEventArgs
    {
        public KeyCode keyCode;
        public KeyEventType type;

        public void Release()
        {
        }

        public static KeyEventArgs Create(KeyCode keyCode, KeyEventType type)
        {
            KeyEventArgs args = GameFrameworkFactory.Spawner<KeyEventArgs>();
            args.keyCode = keyCode;
            args.type = type;
            return args;
        }
    }

    public enum KeyEventType : byte
    {
        Down,
        Up,
        Press
    }

    /// <summary>
    /// 事件通知管理器
    /// </summary>
    public sealed class NotifyManager : GameFrameworkModule
    {
        private bool isTouch = false;
        private List<KeyCode> keyDownEvent = new();
        private Dictionary<object, GameEventGroup> _handlers = new();

        public override void Update()
        {
            CheckKeyDown();
            CheckKeyUp();
            CheckPressKey();
        }

        private void CheckKeyDown()
        {
            for (int i = 0; i < keyDownEvent.Count; i++)
            {
                if (Input.GetKeyDown(keyDownEvent[i]))
                {
                    Notify(keyDownEvent[i], KeyEventArgs.Create(keyDownEvent[i], KeyEventType.Down));
                }
            }
        }

        private void CheckKeyUp()
        {
            for (int i = 0; i < keyDownEvent.Count; i++)
            {
                if (Input.GetKeyUp(keyDownEvent[i]))
                {
                    Notify(keyDownEvent[i], KeyEventArgs.Create(keyDownEvent[i], KeyEventType.Up));
                }                                                                
            }
        }

        private void CheckPressKey()
        {
            for (var i = 0; i < keyDownEvent.Count; i++)
            {
                if (Input.GetKey(keyDownEvent[i]) || Input.touchCount > 0)
                {
                    Notify(keyDownEvent[i], KeyEventArgs.Create(keyDownEvent[i], KeyEventType.Press));
                }
            }
        }

        /// <summary>
        /// 注册事件回调
        /// </summary>
        /// <param name="eventName"></param>
        /// <param name="handle"></param>
        public void Subscribe<T>(Action<T> handle) where T : IGameEventArgs, new()
        {
            Subscribe(typeof(T).Name, GameEventHandle<T>.Create(handle));
        }

        /// <summary>
        /// 注册事件回调
        /// </summary>
        /// <param name="eventName"></param>
        /// <param name="handle"></param>
        public void Subscribe<T>(object eventName, Action<T> handle) where T : IGameEventArgs, new()
        {
            Subscribe(eventName, GameEventHandle<T>.Create(handle));
        }

        /// <summary>
        /// 注册事件回调
        /// </summary>
        /// <param name="handle"></param>
        /// <typeparam name="T"></typeparam>
        public void Subscribe<T>(object eventName, IGameEventHandle<T> handle) where T : IGameEventArgs, new()
        {
            Subscribe(eventName, (IGameEventHandle)handle);
        }

        /// <summary>
        /// 注册事件回调
        /// </summary>
        /// <param name="eventName"></param>
        /// <param name="handle"></param>
        public void Subscribe(object eventName, Action<IGameEventArgs> handle)
        {
            Subscribe(eventName, GameEventHandle<IGameEventArgs>.Create<IGameEventArgs>(handle));
        }

        /// <summary>
        /// 注册事件回调
        /// </summary>
        /// <param name="eventName"></param>
        /// <param name="handle"></param>
        public void Subscribe(object eventName, IGameEventHandle handle)
        {
            if (eventName is KeyCode keyCode)
            {
                keyDownEvent.Add(keyCode);
            }

            if (_handlers.TryGetValue(eventName, out GameEventGroup group) is false)
            {
                _handlers.Add(eventName, group = GameFrameworkFactory.Spawner<GameEventGroup>());
            }

            group.Subscribe(handle);
        }

        /// <summary>
        /// 取消事件回调
        /// </summary>
        /// <param name="eventName"></param>
        /// <param name="handle"></param>
        /// <typeparam name="T"></typeparam>
        public void Unsubscribe<T>(object eventName, Action<T> handle) where T : IGameEventArgs
        {
            if (_handlers.TryGetValue(eventName, out GameEventGroup group) is false)
            {
                return;
            }

            group.Unsubscribe(handle);
        }

        /// <summary>
        /// 取消事件回调
        /// </summary>
        /// <param name="eventName"></param>
        /// <param name="handle"></param>
        public void Unsubscribe(object eventName, Action<IGameEventArgs> handle)
        {
            if (eventName is KeyCode keyCode)
            {
                keyDownEvent.Remove(keyCode);
            }

            if (_handlers.TryGetValue(eventName, out GameEventGroup group) is false)
            {
                return;
            }

            group.Unsubscribe(handle);
        }

        /// <summary>
        /// 取消事件回调
        /// </summary>
        /// <param name="eventName"></param>
        /// <param name="handle"></param>
        public void Unsubscribe(object eventName, IGameEventHandle handle)
        {
            if (eventName is KeyCode keyCode)
            {
                keyDownEvent.Add(keyCode);
            }

            if (_handlers.TryGetValue(eventName, out GameEventGroup group) is false)
            {
                return;
            }

            group.Unsubscribe(handle);
        }

        /// <summary>
        /// 清理所有相同类型的事件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void Clear<T>() where T : IGameEventArgs
        {
            Clear(typeof(T));
        }

        /// <summary>
        /// 清理所有相同类型的事件
        /// </summary>
        /// <param name="type"></param>
        public void Clear(Type type)
        {
            Clear(type.Name);
        }

        /// <summary>
        /// 清理所有相同类型的事件
        /// </summary>
        /// <param name="eventName"></param>
        public void Clear(object eventName)
        {
            if (_handlers.TryGetValue(eventName, out GameEventGroup group) is false)
            {
                return;
            }

            GameFrameworkFactory.Release(group);
            _handlers.Remove(eventName);
        }

        /// <summary>
        /// 通知事件
        /// </summary>
        /// <param name="eventArgs"></param>
        public void Notify(IGameEventArgs eventArgs)
        {
            Notify(eventArgs.GetType(), eventArgs);
        }

        public void Notify(object eventKey, IGameEventArgs data)
        {
            if (_handlers.TryGetValue(eventKey, out GameEventGroup group) is false)
            {
                Debug.LogError("没有找到事件列表：" + eventKey);
                return;
            }

            group.Notify(data);
        }

        public override void Release()
        {
            foreach (var VARIABLE in _handlers.Values)
            {
                GameFrameworkFactory.Release(VARIABLE);
            }

            _handlers.Clear();
        }
    }
}