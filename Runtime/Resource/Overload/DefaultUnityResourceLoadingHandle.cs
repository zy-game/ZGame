using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

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

        public ResObject LoadAsset(string path)
        {
            if (path.StartsWith("Resources") is false)
            {
                return default;
            }

            if (ResObjectCache.instance.TryGetValue(handleName, path, out ResObject handle))
            {
                Debug.Log("缓存资源");
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

            return ResObject.OnCreate(_handle, asset, path);
        }

        public async UniTask<ResObject> LoadAssetAsync(string path)
        {
            if (path.StartsWith("Resources") is false)
            {
                return default;
            }

            if (ResObjectCache.instance.TryGetValue(handleName, path, out ResObject handle))
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

            return ResObject.OnCreate(_handle, asset, path);
        }

        public void Release(string handle)
        {
        }
    }
}