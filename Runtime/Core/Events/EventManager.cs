﻿using System;
using System.Collections.Generic;

namespace ZEngine
{
    internal class EventManager : Single<EventManager>
    {
        private Dictionary<Type, List<ISubscribe>> _subscribes = new Dictionary<Type, List<ISubscribe>>();

        internal void Add<T>(ISubscribe subscribe)
        {
            if (!_subscribes.TryGetValue(typeof(T), out List<ISubscribe> list))
            {
                _subscribes.Add(typeof(T), list = new List<ISubscribe>());
            }

            list.Add(subscribe);
        }

        internal void Remove<T>(object target)
        {
            if (!_subscribes.TryGetValue(typeof(T), out List<ISubscribe> list))
            {
                return;
            }

            ISubscribe subscribe = list.Find(x => x.Equals(target));
            if (subscribe is null)
            {
                return;
            }

            list.Remove(subscribe);
            Engine.Class.Release(subscribe);
        }

        public ISubscribe[] GetSubscribes<T>()
        {
            if (_subscribes.TryGetValue(typeof(T), out List<ISubscribe> list))
            {
                return list.ToArray();
            }

            return default;
        }

        internal void Clear<T>()
        {
            if (!_subscribes.TryGetValue(typeof(T), out List<ISubscribe> list))
            {
                return;
            }

            list.ForEach(Engine.Class.Release);
            list.Clear();
        }
    }
}