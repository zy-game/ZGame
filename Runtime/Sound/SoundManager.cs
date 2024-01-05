using System;
using System.Collections.Generic;
using UnityEngine;
using ZGame;
using ZGame.Resource;

namespace ZGame.Sound
{
    public class SoundManager : Singleton<SoundManager>
    {
        private List<SoundPlayableHandle> _handles = new List<SoundPlayableHandle>();

        private const string EFFECT_SOUND = "EffectSound";
        private const string BACK_MUSIC = "BackMusic";

        protected override void OnAwake()
        {
            this.gameObject.AddComponent<AudioListener>();
            AddPlayer(BACK_MUSIC, true);
            AddPlayer(EFFECT_SOUND, false);
        }

        protected override void OnUpdate()
        {
            for (int i = _handles.Count - 1; i >= 0; i--)
            {
                _handles[i].OnUpdate();
            }
        }

        public void AddPlayer(string name, bool isLoop)
        {
            if (_handles.Exists(x => x.name == name))
            {
                return;
            }

            _handles.Add(new SoundPlayableHandle(name, isLoop));
        }

        public void AddPlayer(SoundPlayableHandle handle)
        {
            _handles.Add(handle);
        }

        public SoundPlayableHandle GetPlayer(string name)
        {
            return _handles.Find(x => x.name == name);
        }

        public SoundPlayableHandle GetPlayerWithClipName(string clipName)
        {
            return _handles.Find(x => x.clipName == clipName);
        }

        public void Remove(string name)
        {
            SoundPlayableHandle handle = GetPlayer(name);
            if (handle is null)
            {
                return;
            }

            handle.Stop();
            _handles.Remove(handle);
        }

        public void PlayBackSound(string clipName, Action<PlayState> playCallback = null)
        {
            SoundPlayableHandle handle = GetPlayer(BACK_MUSIC);
            if (handle is null)
            {
                return;
            }

            handle.Play(clipName, playCallback);
        }

        public void PlayBackSound(AudioClip clip, Action<PlayState> playCallback = null)
        {
            SoundPlayableHandle handle = GetPlayer(BACK_MUSIC);
            if (handle is null)
            {
                return;
            }

            handle.Play(clip, playCallback);
        }

        public void PlayEffectSound(string clipName, Action<PlayState> playCallback = null)
        {
            SoundPlayableHandle handle = GetPlayer(EFFECT_SOUND);
            if (handle is null)
            {
                return;
            }

            handle.Play(clipName, playCallback);
        }

        public void PlayEffectSound(AudioClip clip, Action<PlayState> playCallback = null)
        {
            SoundPlayableHandle handle = GetPlayer(EFFECT_SOUND);
            if (handle is null)
            {
                return;
            }

            handle.Play(clip, playCallback);
        }

        public void PauseSound(string clipName)
        {
            SoundPlayableHandle handle = GetPlayerWithClipName(clipName);
            if (handle is null)
            {
                return;
            }

            handle.Pause();
        }

        public void StopSound(string clipName)
        {
            SoundPlayableHandle handle = GetPlayerWithClipName(clipName);
            if (handle is null)
            {
                return;
            }

            handle.Stop();
        }

        public void StopAll()
        {
            SoundPlayableHandle handle = GetPlayer(EFFECT_SOUND);
            if (handle is null)
            {
                return;
            }

            handle.Stop();
        }

        public void ResumeSound(string clipName)
        {
            SoundPlayableHandle handle = GetPlayerWithClipName(clipName);
            if (handle is null)
            {
                return;
            }

            handle.Resume();
        }

        public void SetVolume(string name, float volume)
        {
            SoundPlayableHandle handle = GetPlayer(name);
            if (handle is null)
            {
                return;
            }

            handle.SetVolume(volume);
        }
    }
}