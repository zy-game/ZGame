using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using ZGame.Resource;

namespace ZGame.Sound
{
    public class SoundPlayer : IDisposable
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

        public SoundPlayer(string name, bool isLoop)
        {
            _source = new GameObject(name).AddComponent<AudioSource>();
            _source.transform.parent = BehaviourScriptable.instance.transform;
            _source.transform.localPosition = Vector3.zero;
            _source.loop = isLoop;
        }


        public void Play(string clipName, Action<PlayState> callback)
        {
            resObject = ResourceManager.instance.LoadAsset(name);
            if (resObject.IsSuccess() is false)
            {
                return;
            }

            Play(resObject.GetAsset<AudioClip>(), x =>
            {
                callback?.Invoke(x);
                if (x == PlayState.Complete)
                {
                    resObject.Release();
                }
            });
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

        public void Dispose()
        {
            Stop();
            GameObject.DestroyImmediate(_source.gameObject);
        }
    }
}