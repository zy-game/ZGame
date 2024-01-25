using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using ZGame.Resource;

namespace ZGame.Sound
{
    public class SoundState
    {
        private AudioClip _clip;
        private PlayState _state;
        private ResObject resObject;
        private Action<PlayState> callback;

        public string name { get; }

        public AudioClip clip
        {
            get
            {
                if (_clip == null)
                {
                    Load();
                }

                return _clip;
            }
        }

        public SoundState(string clip, Action<PlayState> callback)
        {
            this.name = clip;
            this.callback = callback;
        }

        public SoundState(AudioClip clip, Action<PlayState> callback) : this(clip.name, callback)
        {
            this._clip = clip;
        }


        private void Load()
        {
            resObject = ResourceManager.instance.LoadAsset(name);
            if (resObject.IsSuccess() is false)
            {
                return;
            }

            _clip = resObject.GetAsset<AudioClip>();
        }

        public void SetState(PlayState state)
        {
            _state = state;
            callback?.Invoke(state);
        }

        public void Dispose()
        {
            _clip = null;
            resObject?.Release();
            resObject = null;
        }
    }
}