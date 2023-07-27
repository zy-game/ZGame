using System;
using System.Collections.Generic;

namespace ZEngine
{
    public class GameEventArgs<T> : IEventArgs where T : GameEventArgs<T>
    {
        private Queue<object> dataList = new Queue<object>();

        protected T GetEventData<T>()
        {
            if (dataList.Count is 0)
            {
                return default;
            }

            return (T)dataList.Dequeue();
        }

        public virtual void Release()
        {
            dataList.Clear();
            GC.SuppressFinalize(this);
        }

        public static IGameExecuteHandle<object> Execute(params object[] paramsList)
        {
            return Execute(default, paramsList);
        }

        public static IGameExecuteHandle<object> Execute(GameCancelToken executeCancelToken, params object[] paramsList)
        {
            T eventArgs = Engine.Reference.Dequeue<T>();
            foreach (var item in paramsList)
            {
                eventArgs.dataList.Enqueue(item);
            }

            return SubscribeManager.instance.ExecuteGameEvent(eventArgs, executeCancelToken);
        }

        public static IGameCancelToken Subscribe(Action<T> callback)
        {
            GameCancelToken gameCancelToken = Engine.Reference.Dequeue<GameCancelToken>();
            DefaultGameEventSubscribe defaultGameEventSubscribe = DefaultGameEventSubscribe.Create(callback);
            gameCancelToken.Register(() => { Unsubscribe(defaultGameEventSubscribe); });
            SubscribeManager.instance.Add<T>(defaultGameEventSubscribe);
            return gameCancelToken;
        }

        public static IGameCancelToken Subscribe(GameEventSubscribe<T> subscribe)
        {
            GameCancelToken gameCancelToken = Engine.Reference.Dequeue<GameCancelToken>();
            gameCancelToken.Register(() => { Unsubscribe(subscribe); });
            SubscribeManager.instance.Add<T>(subscribe);
            return gameCancelToken;
        }

        public static void Unsubscribe(Action<T> callback)
        {
            DefaultGameEventSubscribe defaultGameEventSubscribe = DefaultGameEventSubscribe.Find(callback);
            if (defaultGameEventSubscribe is null)
            {
                return;
            }

            Unsubscribe(defaultGameEventSubscribe);
            DefaultGameEventSubscribe.Remove(defaultGameEventSubscribe);
        }

        public static void Unsubscribe(GameEventSubscribe<T> subscribe)
        {
            SubscribeManager.instance.Remove(subscribe);
        }


        class DefaultGameEventSubscribe : GameEventSubscribe<T>
        {
            private Action<T> subscribeCallback;

            private static List<DefaultGameEventSubscribe> _defaultGameEventSubscribes = new List<DefaultGameEventSubscribe>();

            public static DefaultGameEventSubscribe Create(Action<T> callback)
            {
                DefaultGameEventSubscribe defaultGameEventSubscribe = Engine.Reference.Dequeue<DefaultGameEventSubscribe>();
                defaultGameEventSubscribe.subscribeCallback = callback;
                _defaultGameEventSubscribes.Add(defaultGameEventSubscribe);
                return defaultGameEventSubscribe;
            }

            public static DefaultGameEventSubscribe Find(Action<T> callback)
            {
                return _defaultGameEventSubscribes.Find(x => x.subscribeCallback == callback);
            }

            public static void Remove(DefaultGameEventSubscribe subscribe)
            {
                _defaultGameEventSubscribes.Remove(subscribe);
            }

            protected override void Execute(T eventArgs)
            {
                try
                {
                    subscribeCallback?.Invoke(eventArgs);
                }
                catch (Exception e)
                {
                    Engine.Console.Log(e);
                    throw;
                }
            }

            public override void Release()
            {
                subscribeCallback = null;
            }
        }
    }
}