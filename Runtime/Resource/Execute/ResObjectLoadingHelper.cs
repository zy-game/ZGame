using System;
using System.IO;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ZGame
{
}
namespace ZGame.Resource
{
    class ResObjectLoadingHelper
    {
        private static readonly ResObject EMPTY_OBJECT = new();
        private const string EDITOR_RESOURCES_PACKAGE = "EDITOR_RESOURCES_PACKAGE";
        private const string NETWORK_RESOURCES_PACKAGE = "NETWORK_RESOURCES_PACKAGE";
        private const string INTERNAL_RESOURCES_PACKAGE = "INTERNAL_RESOURCES_PACKAGE";

        public static ResObject LoadAssetObjectSync(string path, string extension = "")
        {
            if (path.StartsWith("http"))
            {
                throw new NotImplementedException("网络资源请使用异步方法加载资源");
            }

            if (path.StartsWith("Resources"))
            {
                return InternalAssetObjectLoadingSync(path, extension); //Add(INTERNAL_RES_PACKAGE, asset, path);
            }
#if UNITY_EDITOR
            if (ResConfig.instance.resMode == ResourceMode.Editor)
            {
                return AssetDatabaseAssetObjectLoadingSync(path, extension);
            }
#endif
            return ResourcePackageAssetObjectLoadingSync(path, extension);
        }

        public static async UniTask<ResObject> LoadAssetObjectAsync(string path, string extension = "")
        {
            if (path.StartsWith("Resources"))
            {
                return await InternalAssetObjectLoadingAsync(path, extension);
            }

            if (path.StartsWith("http"))
            {
                return await StreamingResourceAssetObjectLoadingAsync(path, extension);
            }
#if UNITY_EDITOR
            if (ResConfig.instance.resMode == ResourceMode.Editor)
            {
                return AssetDatabaseAssetObjectLoadingSync(path, extension);
            }
#endif
            return await ResourcePackageAssetObjectLoadingAsync(path, extension);
        }

        static ResObject InternalAssetObjectLoadingSync(string path, string extension = "")
        {
            if (GameFrameworkEntry.CacheObject.TryGetValue(path, out ResObject handle))
            {
                return handle;
            }

            if (GameFrameworkEntry.CacheObject.TryGetValue(INTERNAL_RESOURCES_PACKAGE, out ResPackage package) is false)
            {
                GameFrameworkEntry.CacheObject.SetCacheData(package = new ResPackage(INTERNAL_RESOURCES_PACKAGE));
            }

            Object assetObject = Resources.Load(path.Substring(10));
            if (assetObject == null)
            {
                return EMPTY_OBJECT;
            }

            ResObject resObject = ResObject.Create(package, assetObject, path);
            GameFrameworkEntry.CacheObject.SetCacheData(resObject);
            return resObject;
        }

        static async UniTask<ResObject> InternalAssetObjectLoadingAsync(string path, string extension = "")
        {
            if (GameFrameworkEntry.CacheObject.TryGetValue(path, out ResObject handle))
            {
                return handle;
            }

            if (GameFrameworkEntry.CacheObject.TryGetValue(INTERNAL_RESOURCES_PACKAGE, out ResPackage package) is false)
            {
                GameFrameworkEntry.CacheObject.SetCacheData(package = new ResPackage(INTERNAL_RESOURCES_PACKAGE));
            }

            Object assetObject = await Resources.LoadAsync(path.Substring(10)).ToUniTask();
            if (assetObject == null)
            {
                return EMPTY_OBJECT;
            }

            ResObject resObject = ResObject.Create(package, assetObject, path);
            GameFrameworkEntry.CacheObject.SetCacheData(resObject);
            return resObject;
        }

        static ResObject AssetDatabaseAssetObjectLoadingSync(string path, string extension = "")
        {
#if UNITY_EDITOR
            if (GameFrameworkEntry.CacheObject.TryGetValue(path, out ResObject handle))
            {
                return handle;
            }

            if (GameFrameworkEntry.CacheObject.TryGetValue(EDITOR_RESOURCES_PACKAGE, out ResPackage package) is false)
            {
                GameFrameworkEntry.CacheObject.SetCacheData(package = new ResPackage(EDITOR_RESOURCES_PACKAGE));
            }

            Object assetObject = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
            if (assetObject == null)
            {
                return EMPTY_OBJECT;
            }

            handle = ResObject.Create(package, assetObject, path);
            GameFrameworkEntry.CacheObject.SetCacheData(handle);
            return handle;
#else
            return EMPTY_OBJECT;
#endif
        }

        static ResObject ResourcePackageAssetObjectLoadingSync(string path, string extension = "")
        {
            if (GameFrameworkEntry.CacheObject.TryGetValue(path, out ResObject handle))
            {
                return handle;
            }

            ResourcePackageManifest manifest = GameFrameworkEntry.Resource.GetResourcePackageManifestWithAssetName(path);
            if (manifest is null)
            {
                Debug.Log("没有找到资源信息配置:" + path);
                return EMPTY_OBJECT;
            }

            if (GameFrameworkEntry.CacheObject.TryGetValue(manifest.name, out ResPackage _handle) is false)
            {
                Debug.Log("资源包未加载：" + manifest.name);
                return EMPTY_OBJECT;
            }

            Object asset = _handle.bundle.LoadAsset(path);
            if (asset == null)
            {
                Debug.Log("加载资源失败：" + _handle.name);
                return EMPTY_OBJECT;
            }

            ResObject resObject = ResObject.Create(_handle, asset, path);
            GameFrameworkEntry.CacheObject.SetCacheData(resObject);
            return resObject;
        }

        static async UniTask<ResObject> ResourcePackageAssetObjectLoadingAsync(string path, string extension = "")
        {
            if (GameFrameworkEntry.CacheObject.TryGetValue(path, out ResObject handle))
            {
                return handle;
            }

            ResourcePackageManifest manifest = GameFrameworkEntry.Resource.GetResourcePackageManifestWithAssetName(path);
            if (manifest is null)
            {
                Debug.Log("没有找到资源信息配置:" + path);
                return EMPTY_OBJECT;
            }

            if (GameFrameworkEntry.CacheObject.TryGetValue(manifest.name, out ResPackage _handle) is false)
            {
                await ResPackageLoadingHelper.LoadingResourcePackageListAsync(manifest);
                return await ResourcePackageAssetObjectLoadingAsync(path);
            }

            Object asset = await _handle.bundle.LoadAssetAsync(Path.GetFileNameWithoutExtension(path)).ToUniTask();
            if (asset == null)
            {
                GameFrameworkEntry.Logger.LogError("加载资源失败：" + path);
            }

            ResObject resObject = ResObject.Create(_handle, asset, path);
            GameFrameworkEntry.CacheObject.SetCacheData(resObject);
            return resObject;
        }

        static async UniTask<ResObject> StreamingResourceAssetObjectLoadingAsync(string path, string extension = "")
        {
            if (GameFrameworkEntry.CacheObject.TryGetValue(path, out ResObject handle))
            {
                return handle;
            }

            if (GameFrameworkEntry.CacheObject.TryGetValue(NETWORK_RESOURCES_PACKAGE, out ResPackage package) is false)
            {
                GameFrameworkEntry.CacheObject.SetCacheData(package = new ResPackage(NETWORK_RESOURCES_PACKAGE));
            }

            Object asset = await GameFrameworkEntry.Network.GetStreamingAsset(path, extension, 0);
            if (asset == null)
            {
                GameFrameworkEntry.Logger.LogError("加载资源失败：" + path);
            }

            ResObject resObject = ResObject.Create(package, asset, path);
            GameFrameworkEntry.CacheObject.SetCacheData(resObject);
            return resObject;
        }
    }
}