using System;
using System.Collections.Generic;

namespace ZEngine
{
    class SubscribeManager : Single<SubscribeManager>
    {
        private Dictionary<Type, List<ISubscribe>> subscribes = new Dictionary<Type, List<ISubscribe>>();

        public void Add<T>(GameEventSubscribe<T> subscribe) where T : IEventArgs
        {
            if (!subscribes.TryGetValue(typeof(T), out List<ISubscribe> list))
            {
                subscribes.Add(typeof(T), list = new List<ISubscribe>());
            }

            list.Add(subscribe);
        }

        public void Remove<T>(GameEventSubscribe<T> subscribe) where T : IEventArgs
        {
            if (!subscribes.TryGetValue(typeof(T), out List<ISubscribe> list))
            {
                return;
            }

            list.Remove(subscribe);
            Engine.Reference.Release(subscribe);
        }

        public void Clear<T>()
        {
            if (!subscribes.TryGetValue(typeof(T), out List<ISubscribe> list))
            {
                return;
            }

            list.Clear();
        }

        public IGameExecuteHandle<object> ExecuteGameEvent<T>(T eventArgs, GameCancelToken gameCancelToken = null) where T : IEventArgs
        {
            if (!subscribes.TryGetValue(typeof(T), out List<ISubscribe> list))
            {
                Engine.Console.Log(GameEngineException.Create($"Not Find Subscribe Event With {typeof(T)}"));
                return default;
            }

            GameEventExecuteHandle defaultGameEventExecuteHandle = Engine.Reference.Dequeue<GameEventExecuteHandle>();
            defaultGameEventExecuteHandle.Execute<T>(eventArgs, gameCancelToken, list.ToArray());
            return defaultGameEventExecuteHandle;
        }
    }
}