using System;

namespace ZGame
{
    public enum PlayState
    {
        None,
        Playing,
        Paused,
        Stopped,
        Complete,
    }

    public class Playable : IDisposable
    {
        public string clipName;
        public Action<PlayState> callback;

        public virtual void Dispose()
        {
            callback = null;
        }
    }
}