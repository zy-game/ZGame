using System;
using System.IO;
using UnityEngine;
using ZEngine.Resource;

namespace ZEngine.Playable
{
    class AudioPlayableHandle : IPlayableHandle
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
                IRequestAssetExecute<AudioClip> requestAssetExecute = Engine.Resource.LoadAsset<AudioClip>(url);
                requestAssetExecute.SetAudioClip(source.gameObject);
            }

            status = Status.Execute;
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
                Engine.Cache.Handle(url, this);
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

        public static AudioPlayableHandle Create(string url, float volume = 1, bool isLoop = false, Switch isCache = Switch.Off)
        {
            AudioPlayableHandle internalAudioPlayableHandle = Activator.CreateInstance<AudioPlayableHandle>();
            internalAudioPlayableHandle.name = Path.GetFileName(url);
            internalAudioPlayableHandle.loop = isLoop;
            internalAudioPlayableHandle.url = url;
            internalAudioPlayableHandle.isCache = isCache;
            internalAudioPlayableHandle.volume = volume;
            internalAudioPlayableHandle.status = Status.None;
            internalAudioPlayableHandle.Play();
            return internalAudioPlayableHandle;
        }
    }
}