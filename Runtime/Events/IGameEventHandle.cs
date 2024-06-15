using System;

namespace ZGame.Events
{
    public interface IGameEventHandle : IReference
    {
        void Invoke(IGameEventArgs args);

        public static IGameEventHandle Create<T>(Action<T> action) where T : IGameEventArgs
        {
            return GameEventHandle.Create(action);
        }

        class GameEventHandle : IGameEventHandle
        {
            private object owner;
            private Action<IGameEventArgs> action;

            public void Release()
            {
                action = null;
            }

            public void Invoke(IGameEventArgs args)
            {
                action?.Invoke(args);
            }

            public override bool Equals(object obj)
            {
                if (obj is GameEventHandle handle)
                {
                    return owner == handle.owner;
                }

                return owner.Equals(obj);
            }

            public static GameEventHandle Create<T>(Action<T> action) where T : IGameEventArgs
            {
                GameEventHandle handle = RefPooled.Alloc<GameEventHandle>();
                handle.action = x => action((T)x);
                handle.owner = action;
                return handle;
            }
        }
    }
}