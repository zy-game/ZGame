using System;
using System.Collections.Generic;
using System.Linq;

namespace ZEngine
{
    class SubscribeManager : Single<SubscribeManager>
    {
        private Dictionary<Type, List<ISubscribe>> subscribes = new Dictionary<Type, List<ISubscribe>>();

        public void Add<T>(int key, ISubscribe<T> subscribe) where T : GameEventArgs<T>
        {
            if (!subscribes.TryGetValue(typeof(T), out List<ISubscribe> list))
            {
                subscribes.Add(typeof(T), list = new List<ISubscribe>());
            }

            list.Add(subscribe);
        }

        public void Remove<T>(int key) where T : GameEventArgs<T>
        {
            if (!subscribes.TryGetValue(typeof(T), out List<ISubscribe> list))
            {
                return;
            }

            ISubscribe subscribe = list.Find(x => x.GetHashCode() == key);
            if (subscribe is null)
            {
                return;
            }

            list.Remove(subscribe);
            Engine.Class.Release(subscribe);
        }

        public void Clear<T>()
        {
            if (!subscribes.TryGetValue(typeof(T), out List<ISubscribe> list))
            {
                return;
            }

            list.ForEach(Engine.Class.Release);
            list.Clear();
        }

        public IExecuteHandle ExecuteGameEvent<T>(T eventArgs) where T : GameEventArgs<T>
        {
            if (!subscribes.TryGetValue(typeof(T), out List<ISubscribe> list))
            {
                Engine.Console.Log(GameEngineException.Create($"Not Find Subscribe Event With {typeof(T)}"));
                return default;
            }

            GameEventExecuteHandle defaultGameEventExecuteHandle = Engine.Class.Loader<GameEventExecuteHandle>();
            defaultGameEventExecuteHandle.Execute(eventArgs, list.ToArray());
            return defaultGameEventExecuteHandle;
        }
    }
}