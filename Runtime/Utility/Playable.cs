using System;

namespace ZGame
{
    public class Playable : IDisposable
    {
        public string clipName;
        public Action<bool> callback;

        public virtual void Dispose()
        {
            callback = null;
        }
    }
}