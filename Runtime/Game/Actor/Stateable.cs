using System;

namespace ZGame.Game
{
    public abstract class Stateable : IDisposable
    {
        public string name { get; }

        public Stateable(string name)
        {
            this.name = name;
        }

        public virtual void OnUpdate()
        {
        }

        public virtual void Active()
        {
        }

        public virtual void Inactive()
        {
        }

        public virtual void Dispose()
        {
        }
    }
}