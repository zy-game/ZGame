using System.Collections.Generic;
using UnityEngine;
using ZEngine.Cache;

namespace ZEngine.Playable
{
    public class PlayableManager : Singleton<PlayableManager>
    {
        private List<IPlayableHandle> workList = new List<IPlayableHandle>();


        /// <summary>
        /// 播放视频
        /// </summary>
        /// <param name="url"></param>
        /// <param name="volume"></param>
        /// <param name="isFullScene"></param>
        /// <param name="isLoop"></param>
        /// <returns></returns>
        public IPlayableHandle PlayVideo(string url, float volume = 1, RenderTexture scene = null, bool isLoop = false, Switch isCache = Switch.Off)
        {
            IPlayableHandle handle = GetPlayableHandle(url);
            if (handle is not null)
            {
                return handle;
            }

            if (Engine.Cache.TryGetValue<IPlayableHandle>(url, out handle))
            {
                handle.Play();
            }
            else
            {
                handle = VideoPlayerHandle.Create(url, volume, scene, isLoop);
            }

            workList.Add(handle);
            return handle;
        }

        /// <summary>
        /// 播放音效
        /// </summary>
        /// <param name="soundName"></param>
        /// <param name="volume"></param>
        /// <param name="isLoop"></param>
        /// <returns></returns>
        public IPlayableHandle PlaySound(string url, float volume = 1, bool isLoop = false, Switch isCache = Switch.Off)
        {
            IPlayableHandle handle = GetPlayableHandle(url);
            if (handle is not null)
            {
                return handle;
            }

            if (Engine.Cache.TryGetValue<IPlayableHandle>(url, out handle))
            {
                handle.Play();
            }
            else
            {
                handle = AudioPlayableHandle.Create(url, volume, isLoop);
            }

            workList.Add(handle);
            return handle;
        }


        /// <summary>
        /// 获取播放器
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public IPlayableHandle GetPlayableHandle(string name)
        {
            IPlayableHandle handle = workList.Find(x => x.name == name || x.url == name);
            if (handle is not null)
            {
                return handle;
            }

            return default;
        }

        public void RemovePlayableHandle(string name)
        {
            IPlayableHandle playableHandle = GetPlayableHandle(name);
            if (playableHandle is null)
            {
                return;
            }

            workList.Remove(playableHandle);
            Engine.Cache.Handle(playableHandle.url, playableHandle);
        }
    }
}