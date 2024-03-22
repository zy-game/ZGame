using System;
using System.Collections.Generic;
using UnityEngine;

namespace ZGame.Notify
{
    /// <summary>
    /// 事件通知管理器
    /// </summary>
    public sealed class NotifyManager : GameFrameworkModule
    {
        private Dictionary<string, GameEventGroup> _handlers = new();

        /// <summary>
        /// 注册事件回调
        /// </summary>
        /// <param name="eventName"></param>
        /// <param name="handle"></param>
        public void Subscribe(string eventName, Action<IGameEventArgs> handle)
        {
            Subscribe(eventName, new GameEventHandle(handle));
        }

        /// <summary>
        /// 注册事件回调
        /// </summary>
        /// <param name="eventName"></param>
        /// <param name="handle"></param>
        public void Subscribe<T>(Action<T> handle) where T : IGameEventArgs
        {
            Subscribe(typeof(T).Name, new GameEventHandle<T>(handle));
        }

        /// <summary>
        /// 注册事件回调
        /// </summary>
        /// <param name="eventName"></param>
        /// <param name="handle"></param>
        public void Subscribe<T>(string eventName, Action<T> handle) where T : IGameEventArgs
        {
            Subscribe(eventName, new GameEventHandle<T>(handle));
        }

        /// <summary>
        /// 注册事件回调
        /// </summary>
        /// <param name="handle"></param>
        /// <typeparam name="T"></typeparam>
        public void Subscribe<T>(string eventName, IGameEventHandle<T> handle) where T : IGameEventArgs
        {
            Subscribe(eventName, (IGameEventHandle)handle);
        }

        /// <summary>
        /// 注册事件回调
        /// </summary>
        /// <param name="eventName"></param>
        /// <param name="handle"></param>
        public void Subscribe(string eventName, IGameEventHandle handle)
        {
            if (_handlers.TryGetValue(eventName, out GameEventGroup group) is false)
            {
                _handlers.Add(eventName, group = new GameEventGroup());
            }

            group.Subscribe(handle);
        }


        /// <summary>
        /// 取消事件回调
        /// </summary>
        /// <param name="eventName"></param>
        /// <param name="handle"></param>
        public void Unsubscribe(string eventName, Action<IGameEventArgs> handle)
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
        /// <typeparam name="T"></typeparam>
        public void Unsubscribe<T>(Action<T> handle) where T : IGameEventArgs
        {
            if (_handlers.TryGetValue(typeof(T).Name, out GameEventGroup group) is false)
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
        /// <typeparam name="T"></typeparam>
        public void Unsubscribe<T>(string eventName, Action<T> handle) where T : IGameEventArgs
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
        public void Unsubscribe(string eventName, IGameEventHandle handle)
        {
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
            if (type.IsSubclassOf(typeof(IGameEventArgs)) is false)
            {
                Debug.LogError("类型不是INotifyArgs的子类：" + type.Name);
                return;
            }

            Clear(type.Name);
        }

        /// <summary>
        /// 清理所有相同类型的事件
        /// </summary>
        /// <param name="eventName"></param>
        public void Clear(string eventName)
        {
            if (_handlers.TryGetValue(eventName, out GameEventGroup group) is false)
            {
                return;
            }

            group.Dispose();
            _handlers.Remove(eventName);
        }

        /// <summary>
        /// 通知事件
        /// </summary>
        /// <param name="eventArgs"></param>
        public void Notify(IGameEventArgs eventArgs)
        {
            Notify(eventArgs.GetType().Name, eventArgs);
        }

        public void Notify(string eventName, IGameEventArgs data)
        {
            if (_handlers.TryGetValue(eventName, out GameEventGroup group) is false)
            {
                Debug.LogError("没有找到事件列表：" + eventName);
                return;
            }

            group.Notify(data);
        }

        public override void Dispose()
        {
            foreach (var VARIABLE in _handlers.Values)
            {
                VARIABLE.Dispose();
            }

            _handlers.Clear();
            GC.SuppressFinalize(this);
        }
    }
}