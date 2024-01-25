using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using ZGame.Resource;

namespace ZGame.Sound
{
    public class SoundPlayer : IDisposable
    {
        private SoundState current;
        private AudioSource _source;
        private Queue<SoundState> waiting;
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

        public SoundPlayer(string name, bool isLoop)
        {
            waiting = new Queue<SoundState>();
            _source = new GameObject(name).AddComponent<AudioSource>();
            _source.transform.parent = BehaviourScriptable.instance.transform;
            _source.transform.localPosition = Vector3.zero;
            _source.loop = isLoop;
        }

        public void OnUpdate()
        {
            if (_source.isPlaying)
            {
                return;
            }

            if (current != null)
            {
                current.SetState(PlayState.Complete);
                current.Dispose();
                current = null;
            }

            if (waiting.Count == 0)
            {
                return;
            }

            OnStartPlay(waiting.Dequeue());
        }

        public void Play(string clipName, bool isNow, Action<PlayState> callback)
        {
            SoundState soundState = new SoundState(clipName, callback);
            if (current is null || isNow)
            {
                OnStartPlay(soundState);
                return;
            }

            waiting.Enqueue(soundState);
        }

        public void Play(AudioClip clip, bool isNow, Action<PlayState> callback)
        {
            SoundState soundState = new SoundState(clip, callback);
            if (current is null || isNow)
            {
                OnStartPlay(soundState);
                return;
            }

            waiting.Enqueue(soundState);
        }

        private void OnStartPlay(SoundState soundState)
        {
            if (_source.isPlaying)
            {
                _source.Stop();
                if (current is not null)
                {
                    current.SetState(PlayState.Complete);
                }
            }

            current = soundState;
            _source.clip = soundState.clip;
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