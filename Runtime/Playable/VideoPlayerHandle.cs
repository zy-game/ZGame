using System;
using System.IO;
using UnityEngine;
using UnityEngine.Video;
using ZEngine.Resource;

namespace ZEngine.Playable
{
    class VideoPlayerHandle : IPlayableHandle
    {
        public string name { get; set; }
        public string url { get; set; }
        public float volume { get; set; }
        public bool loop { get; set; }
        public RenderTexture scene { get; set; }
        public Switch isCache { get; set; }
        public Status status { get; set; }
        private VideoPlayer source;

        public void Play()
        {
            if (source == null)
            {
                source = new GameObject("VideoPlayerHandle").AddComponent<VideoPlayer>();
                if (scene == null)
                {
                    source.renderMode = VideoRenderMode.CameraFarPlane;
                    source.targetCamera = Camera.main;
                }
                else
                {
                    source.renderMode = VideoRenderMode.RenderTexture;
                    source.targetTexture = scene;
                }

                source.loopPointReached += OnComplate;
            }

            status = Status.Execute;
            source.isLooping = loop;
            if (url.StartsWith("http"))
            {
                source.source = VideoSource.Url;
                source.url = url;
                source.Play();
            }
            else
            {
                IRequestAssetExecute<VideoClip> requestAssetExecute = Engine.Resource.LoadAsset<VideoClip>(url);
                requestAssetExecute.SetVideoClip(source.gameObject);
                source.Play();
            }
        }

        private void OnComplate(VideoPlayer player)
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

            source.Play();
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

        public static VideoPlayerHandle Create(string url, float volume = 1, RenderTexture scene = null, bool isLoop = false, Switch isCache = Switch.Off)
        {
            VideoPlayerHandle internalVideoPlayerHandle = Activator.CreateInstance<VideoPlayerHandle>();
            internalVideoPlayerHandle.url = url;
            internalVideoPlayerHandle.name = Path.GetFileName(url);
            internalVideoPlayerHandle.volume = volume;
            internalVideoPlayerHandle.loop = isLoop;
            internalVideoPlayerHandle.scene = scene;
            internalVideoPlayerHandle.isCache = isCache;
            internalVideoPlayerHandle.Play();
            return internalVideoPlayerHandle;
        }
    }
}