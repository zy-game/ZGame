using System;
using System.Collections.Generic;

namespace ZEngine
{
    public class GameEventArgs : IDisposable
    {
        private Queue<object> data;

        public virtual T Dequeue<T>()
        {
            return (T)data.Dequeue();
        }

        public virtual void Dispose()
        {
            data?.Clear();
        }

        public static GameEventArgs Create(params object[] list)
        {
            GameEventArgs gameEventArgs = new GameEventArgs();
            gameEventArgs.data = new Queue<object>();
            for (int i = 0; i < list.Length; i++)
            {
                gameEventArgs.data.Enqueue(list[i]);
            }

            return gameEventArgs;
        }
    }
}