using UnityEngine;

namespace ZEngine.Sound
{
    public interface IAudioPlayExecuteHandle
    {
        AudioClip clip { get; }
        void Pause();
        void Stop();
        void Play();
    }
}