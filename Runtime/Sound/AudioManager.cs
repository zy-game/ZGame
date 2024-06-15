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
    /// <summary>
    /// 音效管理器
    /// </summary>
    public class AudioManager : GameManager
    {
        private List<IAudioPlayable> _handles = new List<IAudioPlayable>();


        public override void OnAwake(params object[] args)
        {
            Create("Background Audio Playable", 1, true);
        }

        protected override void Update()
        {
            for (int i = _handles.Count - 1; i >= 0; i--)
            {
                _handles[i].Update();
            }
        }

        public override void Release()
        {
            Clear();
        }

        /// <summary>
        /// 清理所有播放器
        /// </summary>
        public void Clear()
        {
            foreach (var handle in _handles)
            {
                RefPooled.Free(handle);
            }

            _handles.Clear();
        }

        /// <summary>
        /// 指定的播放器是否在播放
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool HasPlaying(string name)
        {
            IAudioPlayable playable = _handles.Find(x => x.name == name);
            if (playable is null)
            {
                return false;
            }

            return playable.state == PlayState.Playing;
        }


        /// <summary>
        /// 创建音效播放器
        /// </summary>
        /// <param name="name"></param>
        /// <param name="volume"></param>
        /// <param name="isLoop"></param>
        /// <returns></returns>
        public IAudioPlayable Create(string name, float volume, bool isLoop)
        {
            IAudioPlayable result = default;
            _handles.Add(result = IAudioPlayable.Create(name, volume, isLoop));
            return result;
        }

        /// <summary>
        /// 获取指定的音效播放器
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public IAudioPlayable GetPlayable(string name)
        {
            return _handles.Find(x => x.name == name);
        }

        /// <summary>
        /// 获取或创建一个播放器
        /// </summary>
        /// <param name="name"></param>
        /// <param name="volume"></param>
        /// <param name="isLoop"></param>
        /// <returns></returns>
        public IAudioPlayable GetOrCreatePlayable(string name, float volume, bool isLoop)
        {
            IAudioPlayable playable = _handles.Find(x => x.name == name);
            if (playable is not null)
            {
                return playable;
            }

            return Create(name, volume, isLoop);
        }

        /// <summary>
        /// 获取未使用的音效播放器
        /// </summary>
        /// <param name="isLoop"></param>
        /// <returns></returns>
        public IAudioPlayable GetFree(bool isLoop = false)
        {
            IAudioPlayable playable = _handles.Find(x => x.state == PlayState.None);
            if (playable is not null)
            {
                return playable;
            }

            return Create("Audio Clip Playable " + _handles.Count, 1, isLoop);
        }

        /// <summary>
        /// 全部播放器静音
        /// </summary>
        public void MuteAll()
        {
            _handles.ForEach(x => x.Mute());
        }

        /// <summary>
        /// 恢复所有播放器
        /// </summary>
        public void ResumeAll()
        {
            _handles.ForEach(x => x.Resume());
        }

        /// <summary>
        /// 设置所有播放器音量
        /// </summary>
        /// <param name="volume"></param>
        public void SetAllVolume(float volume)
        {
            _handles.ForEach(x => x.SetVolume(volume));
        }

        /// <summary>
        /// 停止所有播放器
        /// </summary>
        public void StopAll()
        {
            _handles.ForEach(x => x.Stop());
        }

        /// <summary>
        /// 播放音效
        /// </summary>
        /// <param name="clipName"></param>
        /// <param name="type"></param>
        /// <param name="isLoop"></param>
        /// <param name="callback"></param>
        public async void PlaySound(string clipName, StreamingAssetType type, bool isLoop, Action<PlayState> callback)
        {
            if (clipName.IsNullOrEmpty())
            {
                callback.Invoke(PlayState.Error);
                return;
            }

            if (type is not StreamingAssetType.Audio_MPEG && type is not StreamingAssetType.Audio_WAV)
            {
                callback.Invoke(PlayState.NotSupported);
                return;
            }

            ResObject resObject = await AppCore.Resource.LoadStreamingAssetAsync(clipName, type);
            if (resObject.IsSuccess() is false)
            {
                return;
            }

            PlaySound(resObject.GetAsset<AudioClip>(), isLoop, x =>
            {
                callback?.Invoke(x);
                if (x is PlayState.Complete)
                {
                    AppCore.Resource.Unload(resObject);
                }
            });
        }

        /// <summary>
        /// 播放音效
        /// </summary>
        /// <param name="clip"></param>
        /// <param name="isLoop"></param>
        /// <param name="callback"></param>
        public void PlaySound(AudioClip clip, bool isLoop, Action<PlayState> callback)
        {
            if (clip == null)
            {
                callback?.Invoke(PlayState.Error);
                return;
            }

            IAudioPlayable playable = GetFree();
            playable.SetClip(clip);
            playable.SetLoop(isLoop);
            playable.SetCallback(callback);
            playable.Play();
        }

        /// <summary>
        /// 播放音效
        /// </summary>
        /// <param name="clipName"></param>
        /// <param name="isLoop"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public async UniTask<PlayState> PlaySound(string clipName, bool isLoop, StreamingAssetType type)
        {
            if (clipName.IsNullOrEmpty())
            {
                return PlayState.Error;
            }

            if (type is not StreamingAssetType.Audio_MPEG && type is not StreamingAssetType.Audio_WAV)
            {
                return PlayState.NotSupported;
            }

            ResObject resObject = await AppCore.Resource.LoadStreamingAssetAsync(clipName, type);
            if (resObject.IsSuccess() is false)
            {
                return PlayState.Error;
            }

            PlayState state = await PlaySound(resObject.GetAsset<AudioClip>(), isLoop);
            AppCore.Resource.Unload(resObject);
            return state;
        }

        /// <summary>
        /// 播放音效
        /// </summary>
        /// <param name="clip"></param>
        /// <param name="isLoop"></param>
        /// <returns></returns>
        public UniTask<PlayState> PlaySound(AudioClip clip, bool isLoop)
        {
            if (clip == null)
            {
                return UniTask.FromResult(PlayState.Error);
            }

            return AppCore.Procedure.Execute<PlayState, PlaySoundProceure>(this, clip, isLoop);
        }

        class PlaySoundProceure : IProcedureAsync<PlayState>
        {
            public UniTask<PlayState> Execute(params object[] args)
            {
                UniTaskCompletionSource<PlayState> taskCompletionSource = new UniTaskCompletionSource<PlayState>();
                AudioClip clip = args[0] as AudioClip;
                bool isLoop = args[1] as bool? ?? false;
                AppCore.Audio.PlaySound(clip, isLoop, x =>
                {
                    if (x is PlayState.Complete)
                    {
                        taskCompletionSource.TrySetResult(x);
                    }
                });
                return taskCompletionSource.Task;
            }

            public void Release()
            {
            }
        }
    }
}