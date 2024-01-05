using System;
using System.Collections.Generic;
using UnityEngine;
using ZGame.Resource;

namespace ZGame.Sound
{
    public class SoundPlayableHandle : IDisposable
    {
        private AudioSource _source;
        private Queue<AudioPlayable> waiting;
        private AudioPlayable current;
        public string name => _source.name;
        public string clipName { get; private set; }

        public float volume => _source.volume;

        public SoundPlayableHandle(string name, bool isLoop)
        {
            waiting = new Queue<AudioPlayable>();
            _source = new GameObject(name).AddComponent<AudioSource>();
            _source.transform.parent = SoundManager.instance.gameObject.transform;
            _source.transform.localPosition = Vector3.zero;
            _source.loop = isLoop;
        }

        public void OnUpdate()
        {
            if (current is not null && _source.isPlaying is false)
            {
                current.callback?.Invoke(PlayState.Complete);
                current.Dispose();
                current = null;
                return;
            }

            if (waiting.Count == 0)
            {
                return;
            }

            current = waiting.Dequeue();
            Player();
        }

        public void Play(string clipName, Action<PlayState> playCallback)
        {
            AudioPlayable playable = new AudioPlayable() { clipName = clipName, callback = playCallback };
            if (current is not null)
            {
                waiting.Enqueue(playable);
                return;
            }

            current = playable;
            Player();
        }

        public void Play(AudioClip clip, Action<PlayState> playCallback)
        {
            AudioPlayable playable = new AudioPlayable() { clip = clip, callback = playCallback };
            if (current is not null)
            {
                waiting.Enqueue(playable);
                return;
            }

            current = playable;
            Player();
        }

        private void Player()
        {
            if (current is null)
            {
                return;
            }

            if (current.clip == null)
            {
                current._resHandle = ResourceManager.instance.LoadAsset(current.clipName);
                if (current._resHandle.IsSuccess() is false)
                {
                    return;
                }

                current.clip = current._resHandle.Get<AudioClip>(null);
            }

            _source.clip = current.clip;
            _source.Play();
            current.callback?.Invoke(PlayState.Playing);
        }

        public void Pause()
        {
            if (current is not null)
            {
                current.callback?.Invoke(PlayState.Paused);
            }

            _source.Pause();
        }

        public void Stop()
        {
            if (current is not null)
            {
                current.callback?.Invoke(PlayState.Stopped);
                current.Dispose();
                current = null;
            }

            waiting.Clear();
            _source.Stop();
        }

        public void Resume()
        {
            _source.Play();
        }

        public void SetVolume(float volume)
        {
            _source.volume = volume;
        }

        public void Dispose()
        {
            Stop();
            GameObject.DestroyImmediate(_source.gameObject);
        }
    }
}