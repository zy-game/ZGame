using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using ZGame;
using ZGame.Resource;

namespace ZGame.Sound
{
    public enum PlayState
    {
        None,
        Playing,
        Paused,
        Complete,
    }

    /// <summary>
    /// 音效管理器
    /// </summary>
    public class AudioPlayerManager : GameFrameworkModule
    {
        private List<AudioSourceHandler> _handles = new List<AudioSourceHandler>();


        public override void OnAwake()
        {
        }


        /// <summary>
        /// 指定的音效是否正在播放
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool HasPlayClip(string name)
        {
            return _handles.Find(x => x.name == name).isPlaying;
        }

        /// <summary>
        /// 将音效加入指定的播放器播放队列
        /// </summary>
        /// <param name="playName"></param>
        /// <param name="clipName"></param>
        /// <param name="isLoop"></param>
        /// <param name="playCallback"></param>
        public void PlaySound(string clipName, bool isLoop = false, Action<PlayState> playCallback = null)
        {
            AudioSourceHandler handle = _handles.Find(x => x.isPlaying is false);
            if (handle is null)
            {
                _handles.Add(handle = new AudioSourceHandler(null, "Audio Player " + _handles.Count, isLoop));
            }

            handle.Play(clipName, playCallback);
        }

        /// <summary>
        /// 将音效加入指定的播放器播放队列
        /// </summary>
        /// <param name="playName"></param>
        /// <param name="clip"></param>
        /// <param name="isLoop"></param>
        /// <param name="playCallback"></param>
        public void PlaySound(AudioClip clip, bool isLoop = false, Action<PlayState> playCallback = null)
        {
            AudioSourceHandler handle = _handles.Find(x => x.isPlaying is false);
            if (handle is null)
            {
                _handles.Add(handle = new AudioSourceHandler(null, "Audio Player " + _handles.Count, isLoop));
            }

            handle.Play(clip, playCallback);
        }

        /// <summary>
        /// 将音效加入指定的播放器播放队列
        /// </summary>
        /// <param name="playName"></param>
        /// <param name="clipName"></param>
        /// <param name="isLoop"></param>
        /// <param name="playCallback"></param>
        public UniTask PlaySoundAsync(string clipName, bool isLoop = false, Action<PlayState> playCallback = null)
        {
            UniTaskCompletionSource tcs = new UniTaskCompletionSource();
            PlaySound(clipName, isLoop, state =>
            {
                if (state is not PlayState.Complete)
                {
                    return;
                }

                tcs.TrySetResult();
            });
            return tcs.Task;
        }

        /// <summary>
        /// 将音效加入指定的播放器播放队列
        /// </summary>
        /// <param name="playName"></param>
        /// <param name="clip"></param>
        /// <param name="isLoop"></param>
        /// <param name="playCallback"></param>
        public UniTask PlaySoundAsync(AudioClip clip, bool isLoop = false, Action<PlayState> playCallback = null)
        {
            UniTaskCompletionSource tcs = new UniTaskCompletionSource();
            PlaySound(clip, isLoop, state =>
            {
                if (state is not PlayState.Complete)
                {
                    return;
                }

                tcs.TrySetResult();
            });
            return tcs.Task;
        }


        /// <summary>
        /// 暂停指定的音效
        /// </summary>
        /// <param name="clipName"></param>
        public void Pause(string clipName)
        {
            AudioSourceHandler handle = _handles.Find(x => x.clipName == clipName);
            if (handle is null)
            {
                return;
            }

            handle.Pause();
        }

        /// <summary>
        /// 暂停所有播放
        /// </summary>
        public void PauseAll()
        {
            foreach (AudioSourceHandler soundPlayer in _handles)
            {
                soundPlayer.Pause();
            }
        }

        /// <summary>
        /// 停止指定的播放器
        /// </summary>
        /// <param name="clipName"></param>
        public void Stop(string clipName)
        {
            AudioSourceHandler handle = _handles.Find(x => x.clipName == clipName);
            if (handle is null)
            {
                return;
            }

            handle.Stop();
        }

        /// <summary>
        /// 停止所有音效的播放
        /// </summary>
        public void StopAll()
        {
            foreach (AudioSourceHandler soundPlayer in _handles)
            {
                soundPlayer.Stop();
            }
        }

        /// <summary>
        /// 恢复指定音效的播放
        /// </summary>
        /// <param name="clipName"></param>
        public void Resume(string clipName)
        {
            AudioSourceHandler handle = _handles.Find(x => x.clipName == clipName);
            if (handle is null)
            {
                return;
            }

            handle.Resume();
        }

        /// <summary>
        /// 恢复所有音效的播放
        /// </summary>
        public void Resume()
        {
            foreach (AudioSourceHandler soundPlayer in _handles)
            {
                soundPlayer.Resume();
            }
        }

        /// <summary>
        /// 设置播放器音量
        /// </summary>
        /// <param name="volume"></param>
        public void SetVolume(float volume)
        {
            foreach (AudioSourceHandler soundPlayer in _handles)
            {
                soundPlayer.SetVolume(volume);
            }
        }

        public override void Dispose()
        {
            StopAll();
            Clear();
            GC.SuppressFinalize(this);
        }

        public void Clear()
        {
            foreach (var handle in _handles)
            {
                handle.Dispose();
            }

            _handles.Clear();
        }
    }
}