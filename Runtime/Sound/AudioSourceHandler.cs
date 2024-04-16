using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using ZGame.VFS;

namespace ZGame.Sound
{
    public class AudioSourceHandler : IReference
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


        public async void Play(AudioClip clip, Action<PlayState> callback)
        {
            if (clip == null)
            {
                callback?.Invoke(PlayState.Complete);
                Debug.Log("clip is null");
                return;
            }

            this.callback = callback;
            _source.clip = clip;
            string name = clip.name;
            Resume();
            ZG.Logger.Log("开始播放：" + name);
            await UniTask.WaitForSeconds(0.1f);
            await UniTask.WaitWhile(() => _source.isPlaying);
            Stop();
            ZG.Logger.Log("播放结束：" + name);
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

        public static AudioSourceHandler Create(GameObject gameObject, string name, bool isLoop)
        {
            AudioSourceHandler handler = RefPooled.Spawner<AudioSourceHandler>();
            if (gameObject == null)
            {
                gameObject = new GameObject(name);
                gameObject.SetParent(null, Vector3.zero, Vector3.zero, Vector3.one);
                GameObject.DontDestroyOnLoad(gameObject);
            }

            if (gameObject.TryGetComponent<AudioSource>(out handler._source) is false)
            {
                handler._source = gameObject.AddComponent<AudioSource>();
            }

            handler._source.loop = isLoop;
            return handler;
        }
    }
}