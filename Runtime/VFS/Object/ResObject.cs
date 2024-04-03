using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;
using ZGame.UI;
using Object = UnityEngine.Object;

namespace ZGame.Resource
{
    public sealed class ResObject : IGameCacheObject
    {
        public static ResObject Empty => new ResObject();
        private object obj;
        private ResPackage parent;

        public string name { get; private set; }

        public int refCount { get; private set; }

        public Object Asset => (Object)obj;

        private ResPackage Parent => parent;

        public bool IsSuccess()
        {
            if (obj != null)
            {
                return true;
            }

            return false;
        }

        public T GetAsset<T>(GameObject gameObject)
        {
            if (obj == null || obj is not Object)
            {
                return default;
            }

            Ref();
            gameObject?.SubscribeDestroyEvent(() => { Unref(); });
            return (T)obj;
        }

        public void Release()
        {
            Debug.Log("Dispose ResObject:" + name);
            for (int i = 0; i < refCount; i++)
            {
                parent?.Unref();
            }

            refCount = 0;
            obj = null;
            parent = null;
            name = String.Empty;
        }

        public void Ref()
        {
            refCount++;
            parent?.Ref();
        }

        public void Unref()
        {
            refCount--;
            parent?.Unref();
        }

        public static ResObject Create(object obj, string path)
        {
            return Create(null, obj, path);
        }

        internal static ResObject Create(ResPackage parent, object obj, string path)
        {
            ResObject resObject = GameFrameworkFactory.Spawner<ResObject>();
            resObject.obj = obj;
            resObject.name = path;
            resObject.parent = parent;
            resObject.refCount = 0;
            return resObject;
        }


        /// <summary>
        /// 加载资源
        /// </summary>
        /// <param name="path">资源路径</param>
        /// <returns>资源加载结果</returns>
        internal static ResObject Create(string path)
        {
            if (path.IsNullOrEmpty())
            {
                return ResObject.Empty;
            }

            if (GameFrameworkEntry.Cache.TryGetValue(path, out ResObject resObject))
            {
                return resObject;
            }

            if (path.StartsWith("Resources"))
            {
                resObject = ResObject.Create(Resources.Load(path.Substring(10)), path);
            }

#if UNITY_EDITOR
            else if (ResConfig.instance.resMode == ResourceMode.Editor && path.StartsWith("Assets"))
            {
                resObject = ResObject.Create(UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path), path);
            }
#endif
            else
            {
                if (GameFrameworkEntry.VFS.TryGetPackageManifestWithAssetName(path, out ResourcePackageManifest manifest) is false)
                {
                    GameFrameworkEntry.Logger.LogError("资源未找到：" + path);
                    return ResObject.Empty;
                }

                if (GameFrameworkEntry.Cache.TryGetValue(manifest.name, out ResPackage package) is false)
                {
                    ResPackage.LoadingAssetBundleSync(manifest);
                    return Create(path);
                }

                resObject = ResObject.Create(package, package.bundle.LoadAsset(path), path);
            }

            if (resObject is not null && resObject.IsSuccess())
            {
                GameFrameworkEntry.Cache.SetCacheData(resObject);
            }

            return resObject;
        }

        /// <summary>
        /// 加载资源
        /// </summary>
        /// <param name="path">资源路径</param>
        /// <returns>资源加载任务</returns>
        internal static async UniTask<ResObject> CreateAsync(string path)
        {
            if (path.IsNullOrEmpty())
            {
                return ResObject.Empty;
            }

            if (GameFrameworkEntry.Cache.TryGetValue(path, out ResObject resObject))
            {
                return resObject;
            }

            if (path.StartsWith("Resources"))
            {
                resObject = ResObject.Create(await Resources.LoadAsync(path.Substring(10)), path);
            }
#if UNITY_EDITOR
            else if (ResConfig.instance.resMode == ResourceMode.Editor)
            {
                resObject = ResObject.Create(UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path), path);
            }
#endif
            else
            {
                if (GameFrameworkEntry.VFS.TryGetPackageManifestWithAssetName(path, out ResourcePackageManifest manifest) is false)
                {
                    GameFrameworkEntry.Logger.LogError("资源未找到：" + path);
                    return ResObject.Empty;
                }

                if (GameFrameworkEntry.Cache.TryGetValue(manifest.name, out ResPackage package) is false)
                {
                    await ResPackage.LoadingAssetBundleAsync(manifest);
                    return await CreateAsync(path);
                }

                resObject = ResObject.Create(package, await package.bundle.LoadAssetAsync(path), path);
            }

            if (resObject is not null && resObject.IsSuccess())
            {
                GameFrameworkEntry.Cache.SetCacheData(resObject);
            }

            return resObject;
        }
    }
}