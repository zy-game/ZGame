using System;
using UnityEngine;
using ZEngine.Resource;

namespace ZEngine.Sound
{
    class AudioPlayerHandle : IReference
    {
        public string name => options.title;
        public string clipName { get; private set; }
        public SoundOptions options { get; private set; }
        private AudioSource source;

        public void SetOptions(SoundOptions options)
        {
            this.options = options;
            source = new GameObject($"Sound_{options.title}").AddComponent<AudioSource>();
            source.mute = options.mute == Switch.On;
            source.volume = this.options.volumen;
            source.priority = this.options.priority;
            source.loop = this.options.loop == Switch.On;
            GameObject.DontDestroyOnLoad(source.gameObject);
        }

        public void Release()
        {
            options = null;
            GameObject.DestroyImmediate(source.gameObject);
            GC.SuppressFinalize(this);
        }

        public void Play(string clipName)
        {
            IRequestAssetExecuteResult<AudioClip> requestAssetExecuteResult = Engine.Resource.LoadAsset<AudioClip>(clipName);
            if (requestAssetExecuteResult.asset == null)
            {
                Engine.Console.Log("加载音效失败:" + clipName);
                return;
            }

            this.clipName = clipName;
            source.clip = requestAssetExecuteResult.asset;
            source.Play();
        }

        public void Pause()
        {
            source.Pause();
        }

        public void Resume()
        {
            source.Play();
        }

        public void Stop()
        {
            source.Stop();
        }
    }
}