using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using ZGame.Resource;

namespace ZGame.Sound
{
    public class SoundPlayableHandle : IDisposable
    {
        private AudioSource _source;
        private AudioPlayable current;
        private Queue<AudioPlayable> waiting;
        public string name => _source.name;

        public bool isPlaying
        {
            get
            {
                if (_source == null)
                {
                    return false;
                }

                return _source.isPlaying;
            }
        }

        public string clipName
        {
            get
            {
                if (_source == null || _source.clip == null)
                {
                    return String.Empty;
                }

                return _source.clip.name;
            }
        }

        public float volume
        {
            get
            {
                if (_source == null)
                {
                    return 0;
                }

                return _source.volume;
            }
        }

        public SoundPlayableHandle(string name, bool isLoop)
        {
            waiting = new Queue<AudioPlayable>();
            _source = new GameObject(name).AddComponent<AudioSource>();
            _source.transform.parent = BehaviourScriptable.instance.transform;
            _source.transform.localPosition = Vector3.zero;
            _source.loop = isLoop;
        }

        public void OnUpdate()
        {
            if (current != null && _source.isPlaying is false)
            {
                current.SetState(PlayState.Complete);
                current.Dispose();
                current = null;
                return;
            }

            if (waiting.Count == 0)
            {
                return;
            }

            Player(waiting.Dequeue());
        }

        public void Play(string clipName, Action<PlayState> playCallback)
        {
            AudioPlayable playable = new AudioPlayable(clipName, playCallback);
            if (current is not null)
            {
                waiting.Enqueue(playable);
                return;
            }

            Player(playable);
        }

        public void Play(AudioClip clip, Action<PlayState> playCallback)
        {
            AudioPlayable playable = new AudioPlayable(clip, playCallback);
            if (current is not null)
            {
                waiting.Enqueue(playable);
                return;
            }

            Player(playable);
        }

        public UniTask Play(string clipName)
        {
            UniTaskCompletionSource playCallback = new UniTaskCompletionSource();
            AudioPlayable playable = new AudioPlayable(clipName, playCallback);
            if (current is not null)
            {
                waiting.Enqueue(playable);
                return playCallback.Task;
            }

            Player(playable);
            return playCallback.Task;
        }

        public UniTask Play(AudioClip clip)
        {
            UniTaskCompletionSource playCallback = new UniTaskCompletionSource();
            AudioPlayable playable = new AudioPlayable(clip, playCallback);
            if (current is not null)
            {
                waiting.Enqueue(playable);
                return playCallback.Task;
            }

            Player(playable);
            return playCallback.Task;
        }

        private void Player(AudioPlayable playable)
        {
            current = playable;
            _source.clip = playable.clip;
            _source.Play();
            current.SetState(PlayState.Playing);
        }

        public void Pause()
        {
            if (current is not null)
            {
                current.SetState(PlayState.Paused);
            }

            _source.Pause();
        }

        public void Stop()
        {
            if (current is not null)
            {
                current.SetState(PlayState.Complete);
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