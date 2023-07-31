using System;
using System.Collections.Generic;
using System.Linq;

namespace ZEngine
{
    public interface ISubscribe : IReference
    {
        void Execute(params object[] args);
    }

    public interface ISubscribe<T> : ISubscribe
    {
        void Execute(T args);
    }

    partial class Subscribe
    {
        private static Dictionary<Type, List<ISubscribe>> subscribes = new Dictionary<Type, List<ISubscribe>>();

        internal static void Add<T>(int key, ISubscribe<T> subscribe) where T : GameEventArgs<T>
        {
            if (!subscribes.TryGetValue(typeof(T), out List<ISubscribe> list))
            {
                subscribes.Add(typeof(T), list = new List<ISubscribe>());
            }

            list.Add(subscribe);
        }

        internal static void Remove<T>(int key) where T : GameEventArgs<T>
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

        internal static void Clear<T>()
        {
            if (!subscribes.TryGetValue(typeof(T), out List<ISubscribe> list))
            {
                return;
            }

            list.ForEach(Engine.Class.Release);
            list.Clear();
        }

        internal static IExecute ExecuteGameEvent<T>(T eventArgs) where T : GameEventArgs<T>
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

    partial class Subscribe : ISubscribe
    {
        private Action callback;


        public void Release()
        {
            callback = null;
        }

        public void Execute(params object[] args)
        {
            callback?.Invoke();
        }

        public static Subscribe Create(Action action)
        {
            Subscribe subscribe = new Subscribe();
            subscribe.callback = action;
            return subscribe;
        }

        public static implicit operator Subscribe(Action action)
        {
            return Subscribe.Create(action);
        }

        public static explicit operator Action(Subscribe subscribe)
        {
            return subscribe.callback;
        }

        public static bool operator ==(Subscribe subscribe, Action action)
        {
            return subscribe.callback.Equals(action);
        }

        public static bool operator !=(Subscribe subscribe, Action action)
        {
            return subscribe.callback.Equals(action) == false;
        }
    }
}