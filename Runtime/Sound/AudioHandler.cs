using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using ZGame.Resource;

namespace ZGame.Sound
{
    public interface IPlayable : IReference
    {
        string name { get; }
        PlayState state { get; }
        void Play();
        void Update();
        UniTask PlayAsync();
    }

    /// <summary>
    /// 音效播放器
    /// </summary>
    public interface IAudioPlayable : IPlayable
    {
        /// <summary>
        /// 当前正在播放的音效片段
        /// </summary>
        AudioClip clip { get; }

        /// <summary>
        /// 播放器音量
        /// </summary>
        float volume { get; }

        /// <summary>
        /// 当前播放器是否循环
        /// </summary>
        bool loop { get; }


        /// <summary>
        /// 静音
        /// </summary>
        void Mute();

        /// <summary>
        /// 恢复播放
        /// </summary>
        void Resume();

        /// <summary>
        /// 设置音量
        /// </summary>
        /// <param name="f"></param>
        void SetVolume(float f);

        /// <summary>
        /// 停止播放
        /// </summary>
        void Stop();

        /// <summary>
        /// 设置播放音效片段
        /// </summary>
        /// <param name="clip"></param>
        void SetClip(AudioClip clip);

        void SetCallback(Action<PlayState> callback);

        /// <summary>
        /// 设置是否循环
        /// </summary>
        /// <param name="isLoop"></param>
        void SetLoop(bool isLoop);

        class PlayableChannel : IAudioPlayable
        {
            private AudioSource audioSource;
            private bool isStopping = false;
            private Action<PlayState> callback;
            public string name { get; private set; }
            public PlayState state { get; private set; }
            public AudioClip clip { get; private set; }
            public float volume { get; private set; }
            public bool loop { get; private set; }

            public void Awake()
            {
                audioSource = new GameObject(name).AddComponent<AudioSource>();
                audioSource.loop = loop;
                audioSource.volume = volume;
            }

            public void SetClip(AudioClip clip)
            {
                this.clip = clip;
            }

            public void SetLoop(bool isLoop)
            {
                this.loop = isLoop;
                audioSource.loop = loop;
            }

            public void SetCallback(Action<PlayState> callback)
            {
                this.callback = callback;
            }

            public void SetVolume(float f)
            {
                volume = f;
                audioSource.volume = volume;
            }

            public void Mute()
            {
                audioSource.volume = 0;
                state = PlayState.Paused;
                callback?.Invoke(state);
            }

            public void Resume()
            {
                audioSource.volume = volume;
                audioSource.loop = loop;

                if (state is not PlayState.Paused)
                {
                    return;
                }

                state = PlayState.Playing;
                callback?.Invoke(state);
                if (isStopping)
                {
                    audioSource.Play();
                }
            }


            public void Stop()
            {
                audioSource.Stop();
                state = PlayState.Complete;
                callback?.Invoke(state);
            }

            public async void Play()
            {
                await PlayAsync();
            }

            public async UniTask PlayAsync()
            {
                if (this.clip == null)
                {
                    return;
                }

                isStopping = false;
                audioSource.clip = clip;
                audioSource.Play();
                await UniTask.DelayFrame(1);
                state = PlayState.Playing;
                callback?.Invoke(state);
            }

            public void Update()
            {
                if (state is not PlayState.Playing)
                {
                    return;
                }

                if (audioSource.isPlaying)
                {
                    return;
                }

                state = PlayState.Complete;
                callback?.Invoke(state);
                state = PlayState.None;
            }


            public void Release()
            {
                this.clip = null;
                GameObject.DestroyImmediate(this.audioSource.gameObject);
            }

            internal static PlayableChannel Create(string name, float volume, bool isLoop)
            {
                PlayableChannel channel = RefPooled.Alloc<PlayableChannel>();
                channel.name = name;
                channel.volume = volume;
                channel.loop = isLoop;
                channel.Awake();
                return channel;
            }
        }

        public static IAudioPlayable Create(string name, float volume, bool isLoop)
        {
            return PlayableChannel.Create(name, volume, isLoop);
        }
    }
}