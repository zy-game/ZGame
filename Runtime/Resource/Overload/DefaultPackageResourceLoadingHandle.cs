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

        public ResObject LoadAsset(string path)
        {
            if (path.StartsWith("Resources") || path.StartsWith("http"))
            {
                return default;
            }

            if (ResObjectCache.instance.TryGetValue(path, out ResObject handle))
            {
                Debug.Log("缓存资源");
                return handle;
            }

            ResourcePackageManifest manifest = PackageManifestManager.instance.GetResourcePackageManifestWithAssetName(path);
            if (manifest is null)
            {
                Debug.Log("没有找到资源信息配置:" + path);
                return default;
            }

            if (PackageHandleCache.instance.TryGetValue(manifest.name, out var _handle) is false)
            {
                Debug.Log("重新加载资源包：" + manifest.name);
                ResourceManager.instance.LoadingPackageListSync(manifest);
                return LoadAsset(path);
            }

            if (path.EndsWith(".unity"))
            {
                return ResObject.OnCreate(_handle, null, path);
            }

            var asset = _handle.bundle.LoadAsset(path);
            if (asset == null)
            {
                Debug.Log("加载资源失败：" + _handle.name);
                return default;
            }

            return ResObject.OnCreate(_handle, asset, path);
        }


        public async UniTask<ResObject> LoadAssetAsync(string path)
        {
            if (path.StartsWith("Resources") || path.StartsWith("http"))
            {
                return default;
            }

            if (ResObjectCache.instance.TryGetValue(path, out ResObject handle))
            {
                return handle;
            }

            ResourcePackageManifest manifest = PackageManifestManager.instance.GetResourcePackageManifestWithAssetName(path);
            if (manifest is null)
            {
                Debug.Log("没有找到资源信息配置:" + path);
                return default;
            }

            if (PackageHandleCache.instance.TryGetValue(manifest.name, out var _handle) is false)
            {
                await ResourceManager.instance.LoadingPackageListAsync(manifest);
                return await LoadAssetAsync(path);
            }

            if (path.EndsWith(".unity"))
            {
                return ResObject.OnCreate(_handle, null, path);
            }

            var asset = await _handle.bundle.LoadAssetAsync(path).ToUniTask();
            if (asset == null)
            {
                return default;
            }

            return ResObject.OnCreate(_handle, asset, path);
        }

        public void Release(string obj)
        {
        }
    }
}