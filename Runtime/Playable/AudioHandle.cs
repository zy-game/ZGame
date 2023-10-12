using System;
using System.IO;
using UnityEngine;
using ZEngine.Resource;

namespace ZEngine.Playable
{
    class AudioHandle : IPlayableHandle
    {
        public string name { get; set; }
        public string url { get; set; }
        public float volume { get; set; }
        public bool loop { get; set; }
        public Status status { get; set; }
        public Switch isCache { get; set; }

        private AudioSource source;


        public void Play()
        {
            if (source == null)
            {
                source = new GameObject("AudioPlayerHandle").AddComponent<AudioSource>();
                GameObject.DontDestroyOnLoad(source.gameObject);
            }

            if (source.clip == null)
            {
                IRequestAssetObjectResult requestAssetObjectResult = ZGame.Resource.LoadAsset(url);
                requestAssetObjectResult.SetAssetObject<AudioSource>(source.gameObject);
            }

            source.loop = loop;
            source.Play();
            WaitFor.Create(source.clip.length + 1, OnComplate);
        }

        private void OnComplate()
        {
            status = Status.Success;
            if (isCache == Switch.On)
            {
                source.gameObject.SetActive(false);
                ZGame.Cache.Handle(url, this);
            }
            else
            {
                Dispose();
            }
        }

        public void Stop()
        {
            if (source == null)
            {
                return;
            }

            source.Stop();
        }

        public void Resume()
        {
            if (source == null)
            {
                return;
            }

            source.UnPause();
        }

        public void Pause()
        {
            if (source == null)
            {
                return;
            }

            source.Pause();
        }

        public void Dispose()
        {
            if (source != null)
            {
                GameObject.DestroyImmediate(source.gameObject);
            }

            name = String.Empty;
            url = String.Empty;
            loop = false;
            isCache = Switch.Off;
            status = Status.None;
            GC.SuppressFinalize(this);
        }

        public static AudioHandle Create(string url, float volume = 1, bool isLoop = false, Switch isCache = Switch.Off)
        {
            AudioHandle internalAudioHandle = Activator.CreateInstance<AudioHandle>();
            internalAudioHandle.name = Path.GetFileName(url);
            internalAudioHandle.loop = isLoop;
            internalAudioHandle.url = url;
            internalAudioHandle.isCache = isCache;
            internalAudioHandle.volume = volume;
            internalAudioHandle.status = Status.None;
            internalAudioHandle.Play();
            return internalAudioHandle;
        }
    }
}