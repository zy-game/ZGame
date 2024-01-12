using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using ZGame.Window;

namespace ZGame.Resource
{
    class DefaultUnityResourceLoadingHandle : IResourceLoadingHandle
    {
        private string handleName = "RESOURCES";

        public DefaultUnityResourceLoadingHandle()
        {
            PackageHandleCache.instance.Add(new PackageHandle(handleName, true));
        }

        public bool Contains(string path)
        {
            return path.StartsWith("Resources");
        }

        public void Dispose()
        {
            PackageHandleCache.instance.Remove(handleName);
            GC.SuppressFinalize(this);
        }

        public ResHandle LoadAsset(string path)
        {
            if (path.StartsWith("Resources") is false)
            {
                return default;
            }

            if (ResHandleCache.instance.TryGetValue(handleName, path, out ResHandle handle))
            {
                return handle;
            }

            if (PackageHandleCache.instance.TryGetValue(handleName, out var _handle) is false)
            {
                return default;
            }

            var asset = Resources.Load(path.Substring(10));
            if (asset == null)
            {
                Debug.Log("资源加载失败");
                return default;
            }

            return ResHandle.OnCreate(_handle, asset, path);
        }

        public async UniTask<ResHandle> LoadAssetAsync(string path)
        {
            if (path.StartsWith("Resources") is false)
            {
                return default;
            }

            if (ResHandleCache.instance.TryGetValue(handleName, path, out ResHandle handle))
            {
                return handle;
            }

            if (PackageHandleCache.instance.TryGetValue(handleName, out var _handle) is false)
            {
                return default;
            }

            UnityEngine.Object asset = await Resources.LoadAsync(path.Substring(10)).ToUniTask();
            if (asset == null)
            {
                return default;
            }

            return ResHandle.OnCreate(_handle, asset, path);
        }

        public void Release(string handle)
        {
        }
    }
}