using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using ZGame.Window;

namespace ZGame.Resource
{
    class InternalResourceLoadingHandle : IResourceLoadingHandle
    {
        private ResourcePackageHandle _handle;

        public InternalResourceLoadingHandle()
        {
            _handle = new ResourcePackageHandle("RESOURCES");
        }

        public void Dispose()
        {
            _handle.Dispose();
            _handle = null;
            GC.SuppressFinalize(this);
        }

        public ResHandle LoadAsset(string path)
        {
            if (path.StartsWith("Resources") is false)
            {
                return default;
            }

            if (_handle.TryGetValue(path, out ResHandle resHandle))
            {
                return resHandle;
            }

            var asset = Resources.Load(path.Substring(10));
            if (asset == null)
            {
                return default;
            }

            _handle.Setup(resHandle = new ResHandle(_handle, asset, path));
            return resHandle;
        }

        public async UniTask<ResHandle> LoadAssetAsync(string path, ILoadingHandle loadingHandle = null)
        {
            if (path.StartsWith("Resources") is false)
            {
                return default;
            }

            if (_handle.TryGetValue(path, out ResHandle resHandle))
            {
                return resHandle;
            }

            UnityEngine.Object asset = await Resources.LoadAsync(path.Substring(10)).ToUniTask(loadingHandle);

            if (asset == null)
            {
                return default;
            }

            _handle.Setup(resHandle = new ResHandle(_handle, asset, path));
            return resHandle;
        }

        public bool Release(ResHandle handle)
        {
            if (_handle.Contains(handle))
            {
                return false;
            }

            _handle.Release(handle);
            return true;
        }
    }
}