using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using ZGame.Resource;

namespace ZGame.Sound
{
    public class AudioSourceHandler : IReferenceObject
    {
        private AudioSource _source;
        private ResObject resObject;
        private Action<PlayState> callback;
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

        public AudioSourceHandler(GameObject gameObject, string name, bool isLoop)
        {
            if (gameObject == null)
            {
                gameObject = new GameObject(name);
                gameObject.SetParent(null, Vector3.zero, Vector3.zero, Vector3.one);
                GameObject.DontDestroyOnLoad(gameObject);
            }

            if (gameObject.TryGetComponent<AudioSource>(out _source) is false)
            {
                _source = gameObject.AddComponent<AudioSource>();
            }

            _source.loop = isLoop;
        }

        public async void Play(AudioClip clip, Action<PlayState> callback)
        {
            this.callback = callback;
            _source.clip = clip;
            Resume();
            await UniTask.Delay((int)clip.length * 1000 + 1000);
            Stop();
        }

        public void Pause()
        {
            _source.Pause();
            callback?.Invoke(PlayState.Paused);
        }

        public void Stop()
        {
            _source.Stop();
            _source.clip = null;
            callback?.Invoke(PlayState.Complete);
            resObject = null;
        }

        public void Resume()
        {
            _source.Play();
            callback?.Invoke(PlayState.Playing);
        }

        public void SetVolume(float volume)
        {
            _source.volume = volume;
        }

        public void Release()
        {
            Stop();
            GameObject.DestroyImmediate(_source.gameObject);
        }
    }
}