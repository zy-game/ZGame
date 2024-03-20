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
        private List<EventHandleGroup> _handlers = new();

        public override void OnAwake()
        {
        }

        /// <summary>
        /// 注册事件回调
        /// </summary>
        /// <param name="handle"></param>
        /// <typeparam name="T"></typeparam>
        public void Subscribe<T>(Action<T> handle) where T : INotifyDatable
        {
            Subsceibe(new CommonNotifyHandle<T>(typeof(T).Name, handle));
        }

        /// <summary>
        /// 注册事件回调
        /// </summary>
        /// <param name="type"></param>
        /// <param name="handle"></param>
        public void Subscribe(Type type, Action<INotifyDatable> handle)
        {
            Subsceibe(new CommonNotifyHandle(type.Name, handle));
        }

        /// <summary>
        /// 注册事件回调
        /// </summary>
        /// <param name="eventName"></param>
        /// <param name="handle"></param>
        public void Subscribe(string eventName, Action<INotifyDatable> handle)
        {
            Subsceibe(new CommonNotifyHandle(eventName, handle));
        }

        /// <summary>
        /// 注册事件回调
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void Subscribe<T>() where T : INotifyHandler, new()
        {
            Subsceibe(new T());
        }

        /// <summary>
        /// 注册事件回调
        /// </summary>
        /// <param name="handle"></param>
        public void Subsceibe(INotifyHandler handle)
        {
            if (_handlers.Exists(x => x.Equals(handle)))
            {
                Debug.LogError("重复注册事件：" + handle.eventName);
                return;
            }

            EventHandleGroup eventHandleGroup = _handlers.Find(x => x.eventName == handle.eventName);
            if (eventHandleGroup is null)
            {
                _handlers.Add(eventHandleGroup = new EventHandleGroup(handle.eventName));
            }

            eventHandleGroup.Add(handle);
        }

        /// <summary>
        /// 移除事件通知回调
        /// </summary>
        /// <param name="handle"></param>
        /// <typeparam name="T"></typeparam>
        public void Unsubscribe<T>(Action<T> handle) where T : INotifyDatable
        {
            EventHandleGroup eventHandleGroup = _handlers.Find(x => x.Contains(handle));
            if (eventHandleGroup is null)
            {
                Debug.LogError("没有找到已注册的事件：" + handle.Method.Name);
                return;
            }

            eventHandleGroup.Remove(handle);
        }

        /// <summary>
        /// 移除事件通知回调
        /// </summary>
        /// <param name="handle"></param>
        public void Unsubscribe(Action<INotifyDatable> handle)
        {
            EventHandleGroup eventHandleGroup = _handlers.Find(x => x.Contains(handle));
            if (eventHandleGroup is null)
            {
                Debug.LogError("没有找到已注册的事件：" + handle.Method.Name);
                return;
            }

            eventHandleGroup.Remove(handle);
        }

        /// <summary>
        /// 移除事件通知回调
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void Unsubscribe<T>() where T : INotifyHandler
        {
            Unsubscribe(typeof(T));
        }

        /// <summary>
        /// 移除事件通知回调
        /// </summary>
        /// <param name="type"></param>
        public void Unsubscribe(Type type)
        {
            if (type.IsSubclassOf(typeof(INotifyHandler)) is false)
            {
                Debug.LogError("类型不是IEventNotifyHandler的子类：" + type.Name);
                return;
            }

            EventHandleGroup eventHandleGroup = _handlers.Find(x => x.Contains(type));
            if (eventHandleGroup is null)
            {
                Debug.LogError("没有找到已注册的事件：" + type.Name);
                return;
            }

            eventHandleGroup.Remove(type);
        }

        /// <summary>
        /// 移除事件通知回调
        /// </summary>
        /// <param name="handle"></param>
        public void Unsubscribe(INotifyHandler handle)
        {
            EventHandleGroup eventHandleGroup = _handlers.Find(x => x.Contains(handle));
            if (eventHandleGroup is null)
            {
                Debug.LogError("没有找到已注册的事件：" + handle.eventName);
                return;
            }

            eventHandleGroup.Remove(handle);
        }

        /// <summary>
        /// 清理所有相同类型的事件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void Clear<T>() where T : INotifyDatable
        {
            Clear(typeof(T));
        }

        /// <summary>
        /// 清理所有相同类型的事件
        /// </summary>
        /// <param name="type"></param>
        public void Clear(Type type)
        {
            if (type.IsSubclassOf(typeof(INotifyDatable)) is false)
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
            EventHandleGroup eventHandleGroup = _handlers.Find(x => x.eventName == eventName);
            if (eventHandleGroup is null)
            {
                Debug.LogError("没有找到事件列表：" + eventName);
                return;
            }

            eventHandleGroup.Dispose();
            _handlers.Remove(eventHandleGroup);
        }

        /// <summary>
        /// 通知事件
        /// </summary>
        /// <param name="datable"></param>
        public void Notify(INotifyDatable datable)
        {
            string eventName = datable.GetType().Name;
            EventHandleGroup eventHandleGroup = _handlers.Find(x => x.eventName == eventName);
            if (eventHandleGroup is null)
            {
                Debug.LogError("没有找到事件列表：" + eventName);
                return;
            }

            eventHandleGroup.Notify(datable);
        }

        public override void Dispose()
        {
            _handlers.ForEach(x => x.Dispose());
            _handlers.Clear();
        }
    }
}