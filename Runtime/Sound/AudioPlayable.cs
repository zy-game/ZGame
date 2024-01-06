using System;
using UnityEngine;
using ZGame.Resource;

namespace ZGame.Sound
{
    public class AudioPlayable : Playable
    {
        private AudioClip _clip;

        public AudioClip clip
        {
            get
            {
                if (_clip == null)
                {
                    Load();
                }

                Debug.Log("音效：" + _clip.name);
                return _clip;
            }
        }

        private PlayState _state;
        private ResHandle _resHandle;
        private Action<PlayState> callback;

        public AudioPlayable(string clipName, Action<PlayState> callback) : base(clipName)
        {
            this.callback = callback;
        }

        public AudioPlayable(AudioClip clip, Action<PlayState> callback) : this(clip.name, callback)
        {
            this._clip = clip;
        }

        private void Load()
        {
            _resHandle = ResourceManager.instance.LoadAsset(name);
            if (_resHandle.IsSuccess() is false)
            {
                return;
            }

            _clip = _resHandle.Get<AudioClip>(null);
        }

        public void SetState(PlayState state)
        {
            _state = state;
            callback?.Invoke(state);
        }

        public override void Dispose()
        {
            base.Dispose();
            _clip = null;
            _resHandle?.Release();
            _resHandle = null;
        }
    }
}