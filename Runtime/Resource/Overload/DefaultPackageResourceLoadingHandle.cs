﻿using System.IO;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using ZGame.Window;

namespace ZGame.Resource
{
    class DefaultPackageResourceLoadingHandle : IResourceLoadingHandle
    {
        public void Dispose()
        {
        }

        public bool Contains(string path)
        {
            return ResourceManager.instance.GetResourcePackageHandleWithAssetPath(path) is not null;
        }

        public ResHandle LoadAsset(string path)
        {
            if (path.StartsWith("Resources") || path.StartsWith("http"))
            {
                return default;
            }

            ResPackageHandle handle = ResourceManager.instance.GetResourcePackageHandleWithAssetPath(path);
            if (handle is null)
            {
                //todo 自动加载被释放的包
                return default;
            }

            if (handle.TryGetValue(path, out ResHandle resHandle))
            {
                return resHandle;
            }

            if (path.EndsWith(".unity"))
            {
                resHandle = ResHandle.OnCreate(handle, null, path);
                handle.Setup(resHandle);
                return resHandle;
            }

            var asset = handle.bundle.LoadAsset(path);
            if (asset == null)
            {
                return default;
            }

            resHandle = ResHandle.OnCreate(handle, asset, path);
            handle.Setup(resHandle);
            return resHandle;
        }


        public async UniTask<ResHandle> LoadAssetAsync(string path)
        {
            if (path.StartsWith("Resources") || path.StartsWith("http"))
            {
                return default;
            }

            ResPackageHandle handle = ResourceManager.instance.GetResourcePackageHandleWithAssetPath(path);
            if (handle is null)
            {
                //todo 自动加载被释放的包
                return default;
            }

            if (handle.TryGetValue(path, out ResHandle resHandle))
            {
                return resHandle;
            }

            if (path.EndsWith(".unity"))
            {
                resHandle = ResHandle.OnCreate(handle, null, path);
                handle.Setup(resHandle);
                return resHandle;
            }

            var asset = await handle.bundle.LoadAssetAsync(path).ToUniTask();
            if (asset == null)
            {
                return default;
            }

            handle.Setup(resHandle = ResHandle.OnCreate(handle, asset, path));
            return resHandle;
        }

        public void Release(string obj)
        {
        }
    }
}