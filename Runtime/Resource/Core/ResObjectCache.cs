using System.Collections.Generic;
using System.IO;
using Cysharp.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

namespace ZGame.Resource
{
    class ResObjectCache : Singleton<ResObjectCache>
    {
        private List<ResObject> cacheList;

        private static readonly ResObject EMPTY_OBJECT = new ResObject(null, null, "");
        private static readonly ResPackage EDITOR_RES_PACKAGE = new ResPackage("EDITOR_RESOURCES_PACKAGE");
        private static readonly ResPackage NETWORK_RES_PACKAGE = new ResPackage("NETWORK_RESOURCES_PACKAGE");
        private static readonly ResPackage INTERNAL_RES_PACKAGE = new ResPackage("INTERNAL_RESOURCES_PACKAGE");

        protected override void OnAwake()
        {
            cacheList = new List<ResObject>();
            ResPackageCache.instance.Add(EDITOR_RES_PACKAGE);
            ResPackageCache.instance.Add(NETWORK_RES_PACKAGE);
            ResPackageCache.instance.Add(INTERNAL_RES_PACKAGE);
        }

        public override void Dispose()
        {
            foreach (var VARIABLE in cacheList)
            {
                VARIABLE.Release(true);
            }

            cacheList.Clear();
        }

        public ResObject Add(ResPackage parent, object obj, string path)
        {
            ResObject resObject = new ResObject(parent, obj, path);
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
                if (cacheList[i].IsSubAsset(resPackage) is false)
                {
                    continue;
                }

                Debug.Log("移除资源:" + cacheList[i].path);
                cacheList[i].Release(true);
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
            if (ResObjectCache.instance.TryGetValue(path, out ResObject handle))
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
            if (BasicConfig.instance.resMode == ResourceMode.Editor)
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
            ResourcePackageManifest manifest = PackageManifestManager.instance.GetResourcePackageManifestWithAssetName(path);
            if (manifest is null)
            {
                Debug.Log("没有找到资源信息配置:" + path);
                return EMPTY_OBJECT;
            }

            if (ResPackageCache.instance.TryGetValue(manifest.name, out var _handle) is false)
            {
                Debug.Log("重新加载资源包：" + manifest.name);
                ResPackageCache.instance.LoadSync(manifest);
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
            if (ResObjectCache.instance.TryGetValue(path, out ResObject handle))
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
                using (UnityWebRequest request = GetUnityWebRequest(path, extension))
                {
                    request.useHttpContinue = true;
                    request.disposeUploadHandlerOnDispose = true;
                    request.disposeDownloadHandlerOnDispose = true;
                    await request.SendWebRequest().ToUniTask();
                    if (request.result is not UnityWebRequest.Result.Success)
                    {
                        return default;
                    }

                    switch (ResType(extension))
                    {
                        case 1:
                            asset = DownloadHandlerTexture.GetContent(request);
                            break;
                        case 2:
                            asset = DownloadHandlerAudioClip.GetContent(request);
                            break;
                        default:
                            asset = new TextAsset(request.downloadHandler.text);
                            break;
                    }

                    return Add(NETWORK_RES_PACKAGE, asset, path);
                }
            }
#if UNITY_EDITOR
            if (BasicConfig.instance.resMode == ResourceMode.Editor)
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
            ResourcePackageManifest manifest = PackageManifestManager.instance.GetResourcePackageManifestWithAssetName(path);
            if (manifest is null)
            {
                Debug.Log("没有找到资源信息配置:" + path);
                return EMPTY_OBJECT;
            }

            if (ResPackageCache.instance.TryGetValue(manifest.name, out var _handle) is false)
            {
                await ResPackageCache.instance.LoadAsync(manifest);
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

        private static UnityWebRequest GetUnityWebRequest(string path, string extension)
        {
            switch (extension)
            {
                case ".png":
                case ".jpg":
                case ".jpeg":
                case ".bmp":
                case ".tga":
                    return UnityWebRequestTexture.GetTexture(path);
                case ".mp3":
                    return UnityWebRequestMultimedia.GetAudioClip(path, AudioType.MPEG);
                case ".wav":
                    return UnityWebRequestMultimedia.GetAudioClip(path, AudioType.WAV);
                case ".ogg":
                    return UnityWebRequestMultimedia.GetAudioClip(path, AudioType.OGGVORBIS);
            }

            return UnityWebRequest.Get(path);
        }

        private static int ResType(string extension)
        {
            switch (extension)
            {
                case ".png":
                case ".jpg":
                case ".jpeg":
                case ".bmp":
                case ".tga":
                    return 1;
                case ".mp3":
                case ".wav":
                case ".ogg":
                    return 2;
                case ".txt":
                case ".json":
                    return 3;
            }

            return 0;
        }
    }
}