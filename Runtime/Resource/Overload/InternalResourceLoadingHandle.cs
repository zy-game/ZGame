using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using ZGame.Window;

namespace ZGame.Resource
{
    class InternalResourceLoadingHandle : IResourceLoadingHandle
    {
        private string handleName = "RESOURCES";

        public InternalResourceLoadingHandle()
        {
            ResourceManager.instance.AddResourcePackageHandle(new ResourcePackageHandle(handleName, true));
        }

        public bool Contains(string path)
        {
            return path.StartsWith("Resources");
        }

        public void Dispose()
        {
            ResourceManager.instance.RemoveResourcePackageHandle(handleName);
            GC.SuppressFinalize(this);
        }

        public ResHandle LoadAsset(string path)
        {
            if (path.StartsWith("Resources") is false)
            {
                return default;
            }

            ResourcePackageHandle _handle = ResourceManager.instance.GetResourcePackageHandle(handleName);
            if (_handle is null)
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

            ResourcePackageHandle _handle = ResourceManager.instance.GetResourcePackageHandle(handleName);
            if (_handle is null)
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

        public void Release(string handle)
        {
        }
    }
}