using System;

namespace ZEngine.Playable
{
    public interface IPlayableHandle : IDisposable
    {
        string name { get; }
        string url { get; }
        float volume { get; }
        bool loop { get; }
        Status status { get; }
        void Play();
        void Stop();
        void Resume();
        void Pause();
    }
}