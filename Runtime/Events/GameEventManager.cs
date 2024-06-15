using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using ZGame.Language;
using ZGame.UI;

namespace ZGame.Events
{
    /// <summary>
    /// 事件通知管理器
    /// </summary>
    public sealed class GameEventManager : GameManager
    {
        private Mapping<object, IGameEventHandle> mapping = new();

        /// <summary>
        /// 注册事件回调
        /// </summary>
        /// <param name="eventName"></param>
        /// <param name="handle"></param>
        public void Subscribe<T>(Action<T> handle) where T : IGameEventArgs
        {
            Subscribe(typeof(T).Name, handle);
        }

        /// <summary>
        /// 注册事件回调
        /// </summary>
        /// <param name="eventName"></param>
        /// <param name="handle"></param>
        public void Subscribe<T>(object eventName, Action<T> handle) where T : IGameEventArgs
        {
            Subscribe(eventName, IGameEventHandle.Create(handle));
        }

        /// <summary>
        /// 注册事件回调
        /// </summary>
        /// <param name="eventName"></param>
        /// <param name="handle"></param>
        public void Subscribe(object eventName, IGameEventHandle handle)
        {
            mapping.Add(eventName, handle);
        }

        /// <summary>
        /// 取消事件回调
        /// </summary>
        /// <param name="eventName"></param>
        /// <param name="handle"></param>
        /// <typeparam name="T"></typeparam>
        public void Unsubscribe<T>(Action<T> handle) where T : IGameEventArgs
        {
            Unsubscribe(typeof(T).Name, handle);
        }

        /// <summary>
        /// 取消事件回调
        /// </summary>
        /// <param name="eventName"></param>
        /// <param name="handle"></param>
        /// <typeparam name="T"></typeparam>
        public void Unsubscribe<T>(object eventName, Action<T> handle) where T : IGameEventArgs
        {
            mapping.RemoveAll(eventName, x => x.Equals(handle));
        }

        /// <summary>
        /// 取消事件回调
        /// </summary>
        /// <param name="eventName"></param>
        /// <param name="handle"></param>
        public void Unsubscribe(object eventName, IGameEventHandle handle)
        {
            mapping.RemoveAll(eventName, x => x.Equals(handle));
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
            mapping.RemoveAll(eventName);
        }

        /// <summary>
        /// 派发事件
        /// </summary>
        /// <param name="eventArgs"></param>
        public void Dispatch(IGameEventArgs eventArgs)
        {
            Dispatch(eventArgs.GetType(), eventArgs);
        }

        /// <summary>
        /// 派发事件
        /// </summary>
        /// <param name="eventName"></param>
        /// <param name="data"></param>
        public void Dispatch(object eventName, IGameEventArgs data)
        {
            if (mapping.TryGetValue(eventName, out IEnumerable<IGameEventHandle> handles) is false)
            {
                return;
            }

            foreach (var handle in handles)
            {
                handle.Invoke(data);
            }
        }

        public override void Release()
        {
            RefPooled.Free(mapping);
        }
    }
}