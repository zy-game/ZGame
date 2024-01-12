using System.IO;
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
            return PackageManifestManager.instance.GetResourcePackageManifestWithAssetName(path) is not null;
        }

        public ResHandle LoadAsset(string path)
        {
            if (path.StartsWith("Resources") || path.StartsWith("http"))
            {
                return default;
            }

            if (PackageHandleCache.instance.TryGetValueWithAssetPath(path, out var _handle) is false)
            {
                ResourcePackageManifest manifest = PackageManifestManager.instance.GetResourcePackageManifestWithAssetName(path);
                if (manifest is null)
                {
                    Debug.Log("没有找到资源信息配置");
                    return default;
                }

                Debug.Log("重新加载资源包：" + manifest.name);
                ResourceManager.instance.LoadingPackageListSync(manifest);
                return LoadAsset(path);
            }

            if (ResHandleCache.instance.TryGetValue(_handle.name, path, out ResHandle handle))
            {
                return handle;
            }

            if (path.EndsWith(".unity"))
            {
                return ResHandle.OnCreate(_handle, null, path);
            }

            var asset = _handle.bundle.LoadAsset(path);
            if (asset == null)
            {
                Debug.Log("加载资源失败：" + _handle.name);
                return default;
            }

            return ResHandle.OnCreate(_handle, asset, path);
        }


        public async UniTask<ResHandle> LoadAssetAsync(string path)
        {
            if (path.StartsWith("Resources") || path.StartsWith("http"))
            {
                return default;
            }

            if (PackageHandleCache.instance.TryGetValueWithAssetPath(path, out var _handle) is false)
            {
                ResourcePackageManifest manifest = PackageManifestManager.instance.GetResourcePackageManifestWithAssetName(path);
                if (manifest is null)
                {
                    Debug.Log("没有找到资源信息配置");
                    return default;
                }

                await ResourceManager.instance.LoadingPackageListAsync(manifest);
                return await LoadAssetAsync(path);
            }

            if (ResHandleCache.instance.TryGetValue(_handle.name, path, out ResHandle handle))
            {
                return handle;
            }

            if (path.EndsWith(".unity"))
            {
                return ResHandle.OnCreate(_handle, null, path);
            }

            var asset = await _handle.bundle.LoadAssetAsync(path).ToUniTask();
            if (asset == null)
            {
                return default;
            }

            return ResHandle.OnCreate(_handle, asset, path);
        }

        public void Release(string obj)
        {
        }
    }
}