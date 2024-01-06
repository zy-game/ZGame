using System;

namespace ZGame
{
    public enum PlayState
    {
        None,
        Playing,
        Paused,
        Complete,
    }

    public class Playable : IDisposable
    {
        public string name { get; }

        public Playable(string name)
        {
            this.name = name;
        }



        public virtual void Dispose()
        {
        }
    }
}