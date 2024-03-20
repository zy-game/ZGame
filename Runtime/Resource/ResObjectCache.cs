using System;
using System.Collections.Generic;
using System.IO;
using Cysharp.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using ZGame.Networking;
using Object = UnityEngine.Object;

namespace ZGame.Resource
{
    class ResourcePackageLoadHelper : IDisposable
    {
        public void Dispose()
        {
        }
    }

    class ResourceObjectLoadHelper : IDisposable
    {
//         public async UniTask<ResObject> LoadAsync(string path)
//         {
//             if (GameFrameworkEntry.Resource.ResObjectCache.TryGetValue(path, out ResObject handle))
//             {
//                 return handle;
//             }
//
//             Debug.Log("Load Assets:" + path);
//             Object asset = default;
//             if (path.StartsWith("Resources"))
//             {
//                 asset = await Resources.LoadAsync(path.Substring(10)).ToUniTask();
//                 if (asset == null)
//                 {
//                     return EMPTY_OBJECT;
//                 }
//
//                 return Add(INTERNAL_RES_PACKAGE, asset, path);
//             }
//
//             if (path.StartsWith("http"))
//             {
//                 asset = await GameFrameworkEntry.Download.GetStreamingAsset(path, extension);
//                 if (asset == null)
//                 {
//                     return EMPTY_OBJECT;
//                 }
//
//                 return Add(NETWORK_RES_PACKAGE, asset, path);
//             }
// #if UNITY_EDITOR
//             if (ResConfig.instance.resMode == ResourceMode.Editor)
//             {
//                 if (path.EndsWith(".unity") is false)
//                 {
//                     asset = UnityEditor.AssetDatabase.LoadAssetAtPath(path, typeof(UnityEngine.Object));
//                     if (asset == null)
//                     {
//                         return EMPTY_OBJECT;
//                     }
//                 }
//
//                 return Add(EDITOR_RES_PACKAGE, asset, path);
//             }
// #endif
//             ResourcePackageManifest manifest = GameFrameworkEntry.Resource.PackageManifest.GetResourcePackageManifestWithAssetName(path);
//             if (manifest is null)
//             {
//                 Debug.Log("没有找到资源信息配置:" + path);
//                 return EMPTY_OBJECT;
//             }
//
//             if (GameFrameworkEntry.Resource.ResPackageCache.TryGetValue(manifest.name, out var _handle) is false)
//             {
//                 await GameFrameworkEntry.Resource.ResPackageCache.LoadingResourcePackageListAsync(manifest);
//                 return await LoadAsync(path);
//             }
//
//             if (path.EndsWith(".unity") is false)
//             {
//                 asset = await _handle.bundle.LoadAssetAsync(path).ToUniTask();
//                 if (asset == null)
//                 {
//                     return EMPTY_OBJECT;
//                 }
//             }
//
//             return Add(_handle, asset, path);
//         }

        public void Dispose()
        {
        }
    }

    internal class ResObjectCache : GameFrameworkModule
    {
        private List<ResObject> cacheList = new List<ResObject>();

        private static readonly ResObject EMPTY_OBJECT = ResObject.Create(null, null, "");
        public static readonly ResPackage EDITOR_RES_PACKAGE = new ResPackage("EDITOR_RESOURCES_PACKAGE");
        public static readonly ResPackage NETWORK_RES_PACKAGE = new ResPackage("NETWORK_RESOURCES_PACKAGE");
        public static readonly ResPackage INTERNAL_RES_PACKAGE = new ResPackage("INTERNAL_RESOURCES_PACKAGE");


        public override void Dispose()
        {
            foreach (var VARIABLE in cacheList)
            {
                VARIABLE.Dispose();
            }

            cacheList.Clear();
        }

        public ResObject Add(ResPackage parent, object obj, string path)
        {
            ResObject resObject = ResObject.Create(parent, obj, path);
            if (obj != null)
            {
                cacheList.Add(resObject);
            }

            return resObject;
        }

        public void Remove(ResObject resObject)
        {
            if (cacheList.Contains(resObject) is false)
            {
                return;
            }

            cacheList.Remove(resObject);
        }

        public void RemovePackage(ResPackage resPackage)
        {
            for (int i = 0; i < cacheList.Count; i++)
            {
                if (cacheList[i].Parent is null)
                {
                    continue;
                }

                if (resPackage.Equals(cacheList[i].Parent) is false)
                {
                    continue;
                }

                Debug.Log("移除资源:" + cacheList[i].path);
                cacheList[i].Dispose();
                cacheList.Remove(cacheList[i]);
            }
        }

        public bool TryGetValue(string path, out ResObject resObject)
        {
            resObject = cacheList.Find(x => x.path == path);
            return resObject is not null;
        }

        public ResObject LoadSync(string path, string extension = "")
        {
            if (TryGetValue(path, out ResObject handle))
            {
                return handle;
            }

            Debug.Log("Load Assets:" + path);
            Object asset = default;
            if (path.StartsWith("http"))
            {
                return EMPTY_OBJECT;
            }

            if (path.StartsWith("Resources"))
            {
                asset = Resources.Load(path.Substring(10));
                if (asset == null)
                {
                    return EMPTY_OBJECT;
                }

                return Add(INTERNAL_RES_PACKAGE, asset, path);
            }
#if UNITY_EDITOR
            if (ResConfig.instance.resMode == ResourceMode.Editor)
            {
                if (path.EndsWith(".unity") is false)
                {
                    asset = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
                    if (asset == null)
                    {
                        return EMPTY_OBJECT;
                    }
                }

                return Add(EDITOR_RES_PACKAGE, asset, path);
            }
#endif
            ResourcePackageManifest manifest = GameFrameworkEntry.Resource.PackageManifest.GetResourcePackageManifestWithAssetName(path);
            if (manifest is null)
            {
                Debug.Log("没有找到资源信息配置:" + path);
                return EMPTY_OBJECT;
            }

            if (GameFrameworkEntry.Resource.ResPackageCache.TryGetValue(manifest.name, out var _handle) is false)
            {
                Debug.Log("重新加载资源包：" + manifest.name);
                GameFrameworkEntry.Resource.ResPackageCache.LoadingResourcePackageListSync(manifest);
                return LoadSync(path);
            }

            if (path.EndsWith(".unity") is false)
            {
                asset = _handle.bundle.LoadAsset(path);
                if (asset == null)
                {
                    Debug.Log("加载资源失败：" + _handle.name);
                    return EMPTY_OBJECT;
                }
            }

            return Add(_handle, asset, path);
        }

        public async UniTask<ResObject> LoadAsync(string path, string extension = "")
        {
            if (TryGetValue(path, out ResObject handle))
            {
                return handle;
            }

            Debug.Log("Load Assets:" + path);
            Object asset = default;
            if (path.StartsWith("Resources"))
            {
                asset = await Resources.LoadAsync(path.Substring(10)).ToUniTask();
                if (asset == null)
                {
                    return EMPTY_OBJECT;
                }

                return Add(INTERNAL_RES_PACKAGE, asset, path);
            }

            if (path.StartsWith("http"))
            {
                asset = await GameFrameworkEntry.Download.GetStreamingAsset(path, extension);
                if (asset == null)
                {
                    return EMPTY_OBJECT;
                }

                return Add(NETWORK_RES_PACKAGE, asset, path);
            }
#if UNITY_EDITOR
            if (ResConfig.instance.resMode == ResourceMode.Editor)
            {
                if (path.EndsWith(".unity") is false)
                {
                    asset = UnityEditor.AssetDatabase.LoadAssetAtPath(path, typeof(UnityEngine.Object));
                    if (asset == null)
                    {
                        return EMPTY_OBJECT;
                    }
                }

                return Add(EDITOR_RES_PACKAGE, asset, path);
            }
#endif
            ResourcePackageManifest manifest = GameFrameworkEntry.Resource.PackageManifest.GetResourcePackageManifestWithAssetName(path);
            if (manifest is null)
            {
                Debug.Log("没有找到资源信息配置:" + path);
                return EMPTY_OBJECT;
            }

            if (GameFrameworkEntry.Resource.ResPackageCache.TryGetValue(manifest.name, out var _handle) is false)
            {
                await GameFrameworkEntry.Resource.ResPackageCache.LoadingResourcePackageListAsync(manifest);
                return await LoadAsync(path);
            }

            if (path.EndsWith(".unity") is false)
            {
                asset = await _handle.bundle.LoadAssetAsync(path).ToUniTask();
                if (asset == null)
                {
                    return EMPTY_OBJECT;
                }
            }

            return Add(_handle, asset, path);
        }
    }
}