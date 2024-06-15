using System;
using System.Collections.Generic;
using System.IO;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using ZGame.Language;
using ZGame.UI;
using Object = UnityEngine.Object;

namespace ZGame.Resource
{
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
            for (int i = 0; i < resObjectCache.Count; i++)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label(resObjectCache[i].name);
                GUILayout.FlexibleSpace();
                GUILayout.Label(resObjectCache[i].refCount.ToString());
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
                if (resObjectCache[i].refCount > 1)
                {
                    continue;
                }

                RefPooled.Free(resObjectCache[i]);
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

        internal static void Unload(ResObject resObject)
        {
            resObject.refCount--;
            resObject.parent?.Unref();
            if (resObject.refCount > 1)
            {
                return;
            }

            resObjectCache.Add(resObject);
            resObjects.Remove(resObject);
        }

        internal static ResObject Create(ResPackage parent, object obj, string path)
        {
            if (obj == null)
            {
                throw new NullReferenceException(nameof(obj));
            }

            ResObject resObject = RefPooled.Alloc<ResObject>();
            resObject.source = obj;
            resObject.name = path;
            resObject.parent = parent;
            resObject.refCount = 1;
            resObjects.Add(resObject);
            return resObject;
        }

        
        internal static ResObject LoadObject<T>(string path) where T : Object
        {
            if (AppCore.Manifest.TryGetAssetFullPath(path, out path) is false)
            {
                return ResObject.DEFAULT;
            }

            if (ResObject.TryGetValue(path, out ResObject resObject))
            {
                return resObject;
            }

            if (path.StartsWith("Resources"))
            {
                resObject = ResObject.Create(ResPackage.DEFAULT, Resources.Load(path.Substring(10)), path);
            }
#if UNITY_EDITOR
            else if (AppCore.resMode == ResourceMode.Editor)
            {
                AppCore.Logger.Log(path);
                resObject = ResObject.Create(ResPackage.DEFAULT, UnityEditor.AssetDatabase.LoadAssetAtPath<T>(path), path);
            }
#endif
            else
            {
                if (AppCore.Manifest.TryGetManifest(path, out ResourcePackageManifest manifest) is false)
                {
                    AppCore.Logger.LogError(new NullReferenceException(path));
                    return ResObject.DEFAULT;
                }

                if (ResPackage.TryGetValue(manifest.name, out ResPackage package) is false)
                {
                    if (ResPackage.LoadBundle(manifest) is not Status.Success)
                    {
                        return ResObject.DEFAULT;
                    }

                    return LoadObject<T>(path);
                }

                resObject = ResObject.Create(package, package.LoadAsset<T>(path), path);
            }

            return resObject;
        }

        internal static async UniTask<ResObject> LoadObjectAsync<T>(string path) where T : Object
        {
            if (AppCore.Manifest.TryGetAssetFullPath(path, out path) is false)
            {
                return ResObject.DEFAULT;
            }

            if (ResObject.TryGetValue(path, out ResObject resObject))
            {
                return resObject;
            }

            if (path.StartsWith("Resources"))
            {
                resObject = ResObject.Create(ResPackage.DEFAULT, await Resources.LoadAsync(path.Substring(10)), path);
            }
#if UNITY_EDITOR
            else if (AppCore.resMode == ResourceMode.Editor)
            {
                AppCore.Logger.Log(path);
                resObject = ResObject.Create(ResPackage.DEFAULT, UnityEditor.AssetDatabase.LoadAssetAtPath<T>(path), path);
            }
#endif
            else
            {
                if (AppCore.Manifest.TryGetManifest(path, out ResourcePackageManifest manifest) is false)
                {
                    AppCore.Logger.LogError("资源未找到：" + path);
                    return ResObject.DEFAULT;
                }

                if (ResPackage.TryGetValue(manifest.name, out ResPackage package) is false)
                {
                    if (await ResPackage.LoadBundleAsync(manifest) is not Status.Success)
                    {
                        return ResObject.DEFAULT;
                    }

                    return await LoadObjectAsync<T>(path);
                }

                resObject = ResObject.Create(package, await package.LoadAssetAsync<T>(path), path);
            }

            return resObject;
        }

        internal static async UniTask<ResObject> LoadSceneAsync(string path, LoadSceneMode mode, IProgress<float> callback)
        {
            UILoading.SetTitle(AppCore.Language.Query(LanguageCode.LoadingGameSceneInfo));
            Scene scene = default;
            ResPackage package = default;
            AsyncOperation operation = default;
            ResObject sceneObject = default;
            LoadSceneParameters parameters = new LoadSceneParameters(LoadSceneMode.Single);

#if UNITY_EDITOR
            if (AppCore.resMode == ResourceMode.Editor)
            {
                operation = UnityEditor.SceneManagement.EditorSceneManager.LoadSceneAsyncInPlayMode(path, parameters);
            }
#endif
            if (operation == null)
            {
                if (AppCore.Manifest.TryGetManifest(path, out ResourcePackageManifest manifest) is false)
                {
                    return ResObject.DEFAULT;
                }

                if (ResPackage.TryGetValue(manifest.name, out package) is false)
                {
                    if (await ResPackage.LoadBundleAsync(manifest) is not Status.Success)
                    {
                        return ResObject.DEFAULT;
                    }

                    return await LoadSceneAsync(path, mode, callback);
                }
                else
                {
                    operation = SceneManager.LoadSceneAsync(Path.GetFileNameWithoutExtension(path), parameters);
                }
            }

            await operation.ToUniTask(UILoading.Show());
            scene = SceneManager.GetSceneAt(SceneManager.sceneCount - 1);
            sceneObject = ResObject.Create(package, scene, path);
            UILoading.Hide();
            return sceneObject;
        }

        internal static async UniTask<ResObject> LoadStreamingObjectAsync(string url, StreamingAssetType assetType, string assetName)
        {
#if UNITY_EDITOR
            using (Watcher watcher = Watcher.Start(nameof(LoadStreamingObjectAsync)))
            {
#endif

                using (UnityWebRequest request = CreateUnityWebRequest(assetType, url))
                {
                    request.SetRequestHeaderWithCors();
                    await request.SendWebRequest();
                    if (request.result is not UnityWebRequest.Result.Success)
                    {
                        AppCore.Logger.LogError(request.error);
                        return ResObject.DEFAULT;
                    }

                    return GetRequestResObject(request, assetType, assetName);
                }
#if UNITY_EDITOR
            }
#endif
        }


        private static ResObject GetRequestResObject(UnityWebRequest request, StreamingAssetType type, string assetName)
        {
            object target = default;
            ResPackage parent = ResPackage.DEFAULT;
            switch (type)
            {
                case StreamingAssetType.Sprite:
                case StreamingAssetType.Texture2D:
                    Texture2D texture2D = DownloadHandlerTexture.GetContent(request);
                    if (type == StreamingAssetType.Sprite)
                    {
                        target = Sprite.Create(texture2D, new Rect(Vector2.zero, new Vector2(texture2D.width, texture2D.height)), Vector2.one / 2);
                    }
                    else
                    {
                        target = texture2D;
                    }

                    break;
                case StreamingAssetType.Audio_MPEG:
                case StreamingAssetType.Audio_WAV:
                    target = DownloadHandlerAudioClip.GetContent(request);
                    break;
                case StreamingAssetType.Briary:
                case StreamingAssetType.Text:
                    target = new TextAsset(request.downloadHandler.text);
                    break;
                case StreamingAssetType.Bundle:
                    if (assetName.IsNullOrEmpty())
                    {
                        throw new NullReferenceException(nameof(assetName));
                    }

                    AssetBundle bundle = DownloadHandlerAssetBundle.GetContent(request);
                    target = bundle.LoadAsset(assetName);
                    parent = ResPackage.Create(bundle);
                    break;
            }

            return ResObject.Create(parent, target, assetName);
        }

        private static UnityWebRequest CreateUnityWebRequest(StreamingAssetType type, string url)
        {
            return type switch
            {
                StreamingAssetType.Sprite => UnityWebRequestTexture.GetTexture(url),
                StreamingAssetType.Texture2D => UnityWebRequestTexture.GetTexture(url),
                StreamingAssetType.Audio_MPEG => UnityWebRequestMultimedia.GetAudioClip(url, AudioType.MPEG),
                StreamingAssetType.Audio_WAV => UnityWebRequestMultimedia.GetAudioClip(url, AudioType.WAV),
                StreamingAssetType.Bundle => UnityWebRequestAssetBundle.GetAssetBundle(url),
                _ => UnityWebRequest.Get(url)
            };
        }
    }
}