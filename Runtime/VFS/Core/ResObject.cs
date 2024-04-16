using System;
using System.Collections.Generic;
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

namespace ZGame.VFS
{
    public partial class ResObject : IReference
    {
        public static ResObject DEFAULT => new ResObject();
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
            if (obj == null)
            {
                Debug.Log("基础资源为空");
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
    }

    public partial class ResObject
    {
        /// <summary>
        /// 缓存池列表
        /// </summary>
        private static List<ResObject> resObjects = new();

        /// <summary>
        /// 缓存池
        /// </summary>
        private static List<ResObject> resObjectCache = new();

#if UNITY_EDITOR
        static internal void OnDrawingGUI()
        {
            GUILayout.BeginVertical(UnityEditor.EditorStyles.helpBox);
            UnityEditor.EditorGUILayout.LabelField("资源对象", UnityEditor.EditorStyles.boldLabel);
            for (int i = 0; i < resObjects.Count; i++)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label(resObjects[i].name);
                GUILayout.FlexibleSpace();
                GUILayout.Label(resObjects[i].refCount.ToString());
                GUILayout.EndHorizontal();
            }

            GUILayout.EndVertical();
            GUILayout.BeginVertical(UnityEditor.EditorStyles.helpBox);
            UnityEditor.EditorGUILayout.LabelField("资源对象池", UnityEditor.EditorStyles.boldLabel);
            for (int i = 0; i < resObjects.Count; i++)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label(resObjects[i].name);
                GUILayout.FlexibleSpace();
                GUILayout.Label(resObjects[i].refCount.ToString());
                GUILayout.EndHorizontal();
            }

            GUILayout.EndVertical();
        }
#endif

        /// <summary>
        /// 检查未引用的对象
        /// </summary>
        internal static void CheckUnusedRefObject()
        {
            for (int i = resObjects.Count - 1; i >= 0; i--)
            {
                if (resObjects[i].refCount > 0)
                {
                    continue;
                }

                resObjectCache.Add(resObjects[i]);
                resObjects.RemoveAt(i);
            }
        }

        /// <summary>
        /// 清理缓存
        /// </summary>
        internal static void ReleaseUnusedRefObject()
        {
            for (int i = resObjectCache.Count - 1; i >= 0; i--)
            {
                if (resObjectCache[i].refCount > 0)
                {
                    continue;
                }

                RefPooled.Release(resObjectCache[i]);
            }

            resObjectCache.Clear();
        }

        internal static bool TryGetValue(string path, out ResObject resObject)
        {
            resObject = resObjects.Find(x => x.name == path);
            if (resObject is null)
            {
                resObject = resObjectCache.Find(x => x.name == path);
                if (resObject is not null)
                {
                    resObjects.Add(resObject);
                    resObjectCache.Remove(resObject);
                }
            }

            return resObject is not null;
        }

        internal static ResObject Create(ResPackage parent, object obj, string path)
        {
            if (obj == null)
            {
                Debug.Log("obj is null");
                return DEFAULT;
            }

            ResObject resObject = RefPooled.Spawner<ResObject>();
            resObject.obj = obj;
            resObject.name = path;
            resObject.parent = parent;
            resObject.refCount = 1;
            return resObject;
        }

        public static ResObject Create(object obj, string path)
        {
            if (obj == null)
            {
                Debug.Log("obj is null");
                return DEFAULT;
            }

            ResObject resObject = new ResObject();
            resObject.obj = obj;
            resObject.name = path;
            resObject.parent = ResPackage.DEFAULT;
            resObject.refCount = 1;
            return resObject;
        }


        /// <summary>
        /// 加载资源
        /// </summary>
        /// <param name="path">资源路径</param>
        /// <returns>资源加载结果</returns>
        internal static ResObject LoadResObjectSync(string path)
        {
            if (path.IsNullOrEmpty())
            {
                return ResObject.DEFAULT;
            }

            if (TryGetValue(path, out ResObject resObject))
            {
                return resObject;
            }

            if (path.StartsWith("Resources"))
            {
                resObject = ResObject.Create(ResPackage.DEFAULT, Resources.Load(path.Substring(10)), path);
            }

#if UNITY_EDITOR
            else if (ResConfig.instance.resMode == ResourceMode.Editor && path.StartsWith("Assets"))
            {
                resObject = ResObject.Create(ResPackage.DEFAULT, UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path), path);
            }
#endif
            else
            {
                if (ZG.VFS.TryGetPackageManifestWithAssetName(path, out ResourcePackageManifest manifest) is false)
                {
                    ZG.Logger.LogError("资源未找到：" + path);
                    return ResObject.DEFAULT;
                }

                if (ResPackage.TryGetValue(manifest.name, out ResPackage package) is false)
                {
                    ResPackage.LoadingAssetBundleSync(manifest);
                    return LoadResObjectSync(path);
                }

                resObject = ResObject.Create(package, package.bundle.LoadAsset(path), path);
            }

            if (resObject is not null && resObject.IsSuccess())
            {
                resObjects.Add(resObject);
            }

            return resObject;
        }

        /// <summary>
        /// 加载资源
        /// </summary>
        /// <param name="path">资源路径</param>
        /// <returns>资源加载任务</returns>
        internal static async UniTask<ResObject> LoadResObjectAsync(string path)
        {
            if (path.IsNullOrEmpty())
            {
                return ResObject.DEFAULT;
            }

            if (TryGetValue(path, out ResObject resObject))
            {
                return resObject;
            }

            if (path.StartsWith("Resources"))
            {
                resObject = ResObject.Create(ResPackage.DEFAULT, await Resources.LoadAsync(path.Substring(10)), path);
            }
#if UNITY_EDITOR
            else if (ResConfig.instance.resMode == ResourceMode.Editor)
            {
                resObject = ResObject.Create(ResPackage.DEFAULT, UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path), path);
            }
#endif
            else
            {
                if (ZG.VFS.TryGetPackageManifestWithAssetName(path, out ResourcePackageManifest manifest) is false)
                {
                    ZG.Logger.LogError("资源未找到：" + path);
                    return ResObject.DEFAULT;
                }

                if (ResPackage.TryGetValue(manifest.name, out ResPackage package) is false)
                {
                    await ResPackage.LoadingAssetBundleAsync(manifest);
                    return await LoadResObjectAsync(path);
                }

                resObject = ResObject.Create(package, await package.bundle.LoadAssetAsync(path), path);
            }

            if (resObject is not null && resObject.IsSuccess())
            {
                resObjects.Add(resObject);
            }

            return resObject;
        }
    }
}