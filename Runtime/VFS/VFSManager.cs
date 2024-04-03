using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using HybridCLR;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using ZGame.Config;
using ZGame.Networking;
using ZGame.Resource;
using ZGame.Resource.Config;
using ZGame.UI;
using Object = UnityEngine.Object;

namespace ZGame.VFS
{
    /// <summary>
    /// 虚拟文件系统
    /// </summary>
    public class VFSManager : GameFrameworkModule
    {
        private NFSManager _disk;
        private const string EDITOR_RESOURCES_PACKAGE = "EDITOR_RESOURCES_PACKAGE";
        private const string NETWORK_RESOURCES_PACKAGE = "NETWORK_RESOURCES_PACKAGE";
        private const string INTERNAL_RESOURCES_PACKAGE = "INTERNAL_RESOURCES_PACKAGE";

        private ResourcePackageManifestManager manifestManager;

        public override void OnAwake(params object[] args)
        {
            manifestManager = new ResourcePackageManifestManager();
            _disk = NFSManager.OpenOrCreateDisk(GameConfig.instance.title + " virtual data.disk");
        }

        public override void Release()
        {
            GameFrameworkFactory.Release(manifestManager);
            GameFrameworkFactory.Release(_disk);
        }

        /// <summary>
        /// 获取资源所在包的清单
        /// </summary>
        /// <param name="assetName"></param>
        /// <param name="manifest"></param>
        /// <returns></returns>
        public bool TryGetPackageManifestWithAssetName(string assetName, out ResourcePackageManifest manifest)
        {
            return manifestManager.TryGetPackageManifestWithAssetName(assetName, out manifest);
        }


        /// <summary>
        /// 删除文件
        /// </summary>
        /// <param name="fileName"></param>
        public void Delete(string fileName)
        {
            _disk.Delete(fileName);
        }

        /// <summary>
        /// 是否存在文件
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="version"></param>
        /// <returns></returns>
        public bool Exist(string fileName, uint version)
        {
            return _disk.Exists(fileName, version);
        }

        /// <summary>
        /// 是否存在文件
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public bool Exist(string fileName)
        {
            return _disk.Exists(fileName);
        }

        /// <summary>
        /// 写入文件数据
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="bytes"></param>
        /// <param name="version"></param>
        public Status Write(string fileName, byte[] bytes, uint version)
        {
            return _disk.Write(fileName, bytes, version);
        }

        /// <summary>
        /// 写入文件数据
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="bytes"></param>
        /// <param name="version"></param>
        public UniTask<Status> WriteAsync(string fileName, byte[] bytes, uint version)
        {
            return _disk.WriteAsync(fileName, bytes, version);
        }

        /// <summary>
        /// 读取文件数据
        /// </summary>
        /// <param name="fileName">文件名</param>
        /// <param name="version">文件版本，当为0时，忽略版本控制</param>
        /// <returns>文件数据</returns>
        public byte[] Read(string fileName, uint version = 0)
        {
            return _disk.Read(fileName);
        }

        /// <summary>
        /// 读取文件数据
        /// </summary>
        /// <param name="fileName">文件名</param>
        /// <param name="version">文件版本，当为0时，忽略版本控制</param>
        /// <returns>文件数据</returns>
        public UniTask<byte[]> ReadAsync(string fileName, uint version = 0)
        {
            return _disk.ReadAsync(fileName);
        }

        /// <summary>
        /// 加载资源包
        /// </summary>
        /// <param name="packageName"></param>
        /// <returns></returns>
        public async UniTask<Status> LoadingResourcePackageAsync(string packageName)
        {
            if (await manifestManager.LoadingPackageManifestData(packageName) is not Status.Success)
            {
                return Status.Fail;
            }

            if (await ResPackage.UpdateResourcePackageList(manifestManager.GetUpdateResourcePackageList(packageName).ToArray()) is not Status.Success)
            {
                return Status.Fail;
            }

            if (await ResPackage.LoadingResourcePackageListAsync(manifestManager.GetResourcePackageAndDependencyList(packageName).ToArray()) is not Status.Success)
            {
                return Status.Fail;
            }

            return Status.Success;
        }

        /// <summary>
        /// 卸载资源包
        /// </summary>
        /// <param name="packageNameList"></param>
        public void UnloadPackages(params string[] packageNameList)
        {
            if (packageNameList is null || packageNameList.Length == 0)
            {
                return;
            }
        }

        public void UnloadResource(Object obj)
        {
        }


        /// <summary>
        /// 加载场景
        /// </summary>
        /// <param name="path">场景路径</param>
        /// <param name="callback">场景加载进度回调</param>
        /// <param name="mode">场景加载模式</param>
        /// <returns></returns>
        public async UniTask<ResObject> GetSceneAsync(string path, IProgress<float> callback, LoadSceneMode mode = LoadSceneMode.Single)
        {
            if (path.IsNullOrEmpty())
            {
                return ResObject.Empty;
            }

            if (GameFrameworkEntry.Cache.TryGetValue(path, out ResObject sceneObject))
            {
                return sceneObject;
            }

            UILoading.SetTitle(GameFrameworkEntry.Language.Query("加载场景中..."));
            Scene scene = default;
            ResPackage package = default;
            AsyncOperation operation = default;
            LoadSceneParameters parameters = new LoadSceneParameters(LoadSceneMode.Single);
#if UNITY_EDITOR
            if (ResConfig.instance.resMode == ResourceMode.Editor)
            {
                operation = UnityEditor.SceneManagement.EditorSceneManager.LoadSceneAsyncInPlayMode(path, parameters);
            }
#endif
            if (operation == null)
            {
                if (manifestManager.TryGetPackageManifestWithAssetName(path, out ResourcePackageManifest manifest) is false)
                {
                    return sceneObject;
                }

                if (GameFrameworkEntry.Cache.TryGetValue(manifest.name, out package) is false)
                {
                    await ResPackage.LoadingResourcePackageListAsync(manifest);
                    return await GetSceneAsync(path, callback, mode);
                }
                else
                {
                    operation = SceneManager.LoadSceneAsync(Path.GetFileNameWithoutExtension(path), parameters);
                }
            }

            await operation.ToUniTask(UILoading.Show());
            scene = SceneManager.GetSceneAt(SceneManager.sceneCount - 1);
            GameFrameworkEntry.Cache.SetCacheData(sceneObject = ResObject.Create(package, scene, path));
            UILoading.Hide();
            return sceneObject;
        }

        /// <summary>
        /// 加载预制体
        /// </summary>
        /// <param name="path">预制体路径</param>
        /// <returns></returns>
        public GameObject GetGameObjectSync(string path)
        {
            ResObject resObject = ResObject.Create(path);
            if (resObject.IsSuccess() is false)
            {
                Debug.LogError("加载资源失败：" + path);
                return default;
            }

            GameObject gameObject = (GameObject)GameObject.Instantiate(resObject.GetAsset<Object>(null));
            gameObject.SubscribeDestroyEvent(() => { resObject.Release(); });
            return gameObject;
        }

        /// <summary>
        /// 加载预制体
        /// </summary>
        /// <param name="path">预制体路径</param>
        /// <param name="parent">初始化时的父物体</param>
        /// <param name="pos">初始化的位置</param>
        /// <param name="rot">初始化的旋转</param>
        /// <param name="scale">初始化时的缩放</param>
        /// <returns></returns>
        /// <returns></returns>
        public GameObject GetGameObjectSync(string path, GameObject parent, Vector3 pos, Vector3 rot, Vector3 scale)
        {
            GameObject gameObject = GetGameObjectSync(path);
            if (gameObject != null)
            {
                if (parent != null)
                {
                    gameObject.SetParent(parent.transform, pos, rot, scale);
                }
                else
                {
                    gameObject.SetParent(null, pos, rot, scale);
                }
            }

            return gameObject;
        }

        /// <summary>
        /// 加载预制体
        /// </summary>
        /// <param name="path">预制体路径</param>
        /// <returns></returns>
        public async UniTask<GameObject> GetGameObjectAsync(string path)
        {
            ResObject resObject = await ResObject.CreateAsync(path);
            if (resObject.IsSuccess() is false)
            {
                Debug.LogError("加载资源失败：" + path);
                return default;
            }

            GameObject gameObject = (GameObject)GameObject.Instantiate(resObject.GetAsset<Object>(null));
            gameObject.SubscribeDestroyEvent(() => { resObject.Release(); });
            return gameObject;
        }

        /// <summary>
        /// 加载预制体
        /// </summary>
        /// <param name="path">预制体路径</param>
        /// <param name="parent">初始化时的父物体</param>
        /// <param name="pos">初始化的位置</param>
        /// <param name="rot">初始化的旋转</param>
        /// <param name="scale">初始化时的缩放</param>
        /// <returns></returns>
        public async UniTask<GameObject> GetGameObjectAsync(string path, GameObject parent, Vector3 pos, Vector3 rot, Vector3 scale)
        {
            GameObject gameObject = await GetGameObjectAsync(path);
            if (gameObject != null)
            {
                if (parent != null)
                {
                    gameObject.SetParent(parent.transform, pos, rot, scale);
                }
                else
                {
                    gameObject.SetParent(null, pos, rot, scale);
                }
            }

            return gameObject;
        }

        /// <summary>
        /// 加载音效
        /// </summary>
        /// <param name="path">音效路径</param>
        /// <param name="gameObject">引用持有者</param>
        /// <returns></returns>
        /// <exception cref="NullReferenceException"></exception>
        /// <exception cref="NotSupportedException"></exception>
        public AudioClip GetAudioClipSync(string path, GameObject gameObject)
        {
            if (path.IsNullOrEmpty())
            {
                throw new NullReferenceException(nameof(path));
            }

            if (path.StartsWith("http"))
            {
                throw new NotSupportedException();
            }

            ResObject resObject = ResObject.Create(path);
            if (resObject.IsSuccess() is false)
            {
                Debug.LogError("加载资源失败：" + path);
                return default;
            }

            return resObject.GetAsset<AudioClip>(gameObject);
        }

        /// <summary>
        /// 加载音效
        /// </summary>
        /// <param name="path">音效路径</param>
        /// <param name="gameObject">引用持有者</param>
        /// <returns></returns>
        /// <exception cref="NullReferenceException"></exception>
        public async UniTask<AudioClip> GetAudioClipAsync(string path, AudioType audioType, GameObject gameObject)
        {
            if (path.IsNullOrEmpty())
            {
                throw new NullReferenceException(nameof(path));
            }

            if (GameFrameworkEntry.Cache.TryGetValue(path, out ResObject resObject) is false)
            {
                if (path.StartsWith("http") is false)
                {
                    resObject = await ResObject.CreateAsync(path);
                }
                else
                {
                    using (UnityWebRequest request = UnityWebRequestMultimedia.GetAudioClip(path, audioType))
                    {
                        await request.SendWebRequest();
                        if (request.result is UnityWebRequest.Result.Success)
                        {
                            resObject = ResObject.Create(DownloadHandlerAudioClip.GetContent(request), path);
                            GameFrameworkEntry.Cache.SetCacheData(resObject);
                        }
                    }
                }
            }

            if (resObject is null || resObject.IsSuccess() is false)
            {
                Debug.LogError("加载资源失败：" + path);
                return default;
            }

            return resObject.GetAsset<AudioClip>(gameObject);
        }

        /// <summary>
        /// 加载图片
        /// </summary>
        /// <param name="path">图片路径</param>
        /// <param name="gameObject">引用持有者</param>
        /// <returns></returns>
        /// <exception cref="NullReferenceException"></exception>
        /// <exception cref="NotSupportedException"></exception>
        public Texture2D GetTexture2DSync(string path, GameObject gameObject)
        {
            if (path.IsNullOrEmpty())
            {
                throw new NullReferenceException(nameof(path));
            }

            if (path.StartsWith("http"))
            {
                throw new NotSupportedException();
            }

            ResObject resObject = ResObject.Create(path);
            if (resObject.IsSuccess() is false)
            {
                Debug.LogError("加载资源失败：" + path);
                return default;
            }

            return resObject.GetAsset<Texture2D>(gameObject);
        }

        /// <summary>
        /// 加载图片
        /// </summary>
        /// <param name="path">图片路径</param>
        /// <param name="gameObject">引用持有者</param>
        /// <returns></returns>
        /// <exception cref="NullReferenceException"></exception>
        /// <exception cref="NotSupportedException"></exception>
        public async UniTask<Texture2D> GetTexture2DAsync(string path, GameObject gameObject)
        {
            if (path.IsNullOrEmpty())
            {
                throw new NullReferenceException(nameof(path));
            }

            if (GameFrameworkEntry.Cache.TryGetValue(path, out ResObject resObject) is false)
            {
                if (path.StartsWith("http") is false)
                {
                    resObject = await ResObject.CreateAsync(path);
                }
                else
                {
                    using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(path))
                    {
                        await request.SendWebRequest();
                        if (request.result is UnityWebRequest.Result.Success)
                        {
                            resObject = ResObject.Create(DownloadHandlerTexture.GetContent(request), path);
                            GameFrameworkEntry.Cache.SetCacheData(resObject);
                        }
                    }
                }
            }

            if (resObject is null || resObject.IsSuccess() is false)
            {
                Debug.LogError("加载资源失败：" + path);
                return default;
            }

            return resObject.GetAsset<Texture2D>(gameObject);
        }

        /// <summary>
        /// 加载精灵图片
        /// </summary>
        /// <param name="path">图片路径</param>
        /// <param name="gameObject">引用持有者</param>
        /// <returns></returns>
        /// <exception cref="NullReferenceException"></exception>
        /// <exception cref="NotSupportedException"></exception>
        public Sprite GetSpriteSync(string path, GameObject gameObject)
        {
            if (path.IsNullOrEmpty())
            {
                throw new NullReferenceException(nameof(path));
            }

            if (path.StartsWith("http"))
            {
                throw new NotSupportedException();
            }

            ResObject resObject = ResObject.Create(path);
            if (resObject.IsSuccess() is false)
            {
                Debug.LogError("加载资源失败：" + path);
                return default;
            }

            return resObject.GetAsset<Sprite>(gameObject);
        }

        /// <summary>
        /// 加载精灵图片
        /// </summary>
        /// <param name="path">图片路径</param>
        /// <param name="gameObject">引用持有者</param>
        /// <returns></returns>
        /// <exception cref="NullReferenceException"></exception>
        public async UniTask<Sprite> GetSpriteAsync(string path, GameObject gameObject)
        {
            if (path.IsNullOrEmpty())
            {
                throw new NullReferenceException(nameof(path));
            }

            if (GameFrameworkEntry.Cache.TryGetValue(path, out ResObject resObject) is false)
            {
                if (path.StartsWith("http") is false)
                {
                    resObject = await ResObject.CreateAsync(path);
                }
                else
                {
                    using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(path))
                    {
                        await request.SendWebRequest();
                        if (request.result is UnityWebRequest.Result.Success)
                        {
                            Texture2D texture2D = DownloadHandlerTexture.GetContent(request);
                            Sprite sp = Sprite.Create(texture2D, new Rect(0, 0, texture2D.width, texture2D.height), new Vector2(0.5f, 0.5f));
                            resObject = ResObject.Create(sp, path);
                            GameFrameworkEntry.Cache.SetCacheData(resObject);
                        }
                    }
                }
            }

            if (resObject is null || resObject.IsSuccess() is false)
            {
                Debug.LogError("加载资源失败：" + path);
                return default;
            }

            return resObject.GetAsset<Sprite>(gameObject);
        }

        /// <summary>
        /// 加载材质球
        /// </summary>
        /// <param name="path">材质球路径</param>
        /// <param name="gameObject">引用持有者</param>
        /// <returns></returns>
        /// <exception cref="NullReferenceException"></exception>
        public Material GetMaterialSync(string path, GameObject gameObject)
        {
            if (path.IsNullOrEmpty())
            {
                throw new NullReferenceException(nameof(path));
            }

            ResObject resObject = ResObject.Create(path);
            if (resObject.IsSuccess() is false)
            {
                Debug.LogError("加载资源失败：" + path);
                return default;
            }

            return resObject.GetAsset<Material>(gameObject);
        }

        /// <summary>
        /// 加载材质球
        /// </summary>
        /// <param name="path">材质球路径</param>
        /// <param name="gameObject">引用持有者</param>
        /// <returns></returns>
        /// <exception cref="NullReferenceException"></exception>
        public async UniTask<Material> GetMaterialAsync(string path, GameObject gameObject)
        {
            if (path.IsNullOrEmpty())
            {
                throw new NullReferenceException(nameof(path));
            }

            ResObject resObject = await ResObject.CreateAsync(path);
            if (resObject.IsSuccess() is false)
            {
                Debug.LogError("加载资源失败：" + path);
                return default;
            }

            return resObject.GetAsset<Material>(gameObject);
        }

        /// <summary>
        /// 获取文本资源
        /// </summary>
        /// <param name="url"></param>
        /// <param name="gameObject"></param>
        /// <returns></returns>
        /// <exception cref="NullReferenceException"></exception>
        public TextAsset GetTextAssetSync(string url, GameObject gameObject)
        {
            if (url.IsNullOrEmpty())
            {
                throw new NullReferenceException(nameof(url));
            }

            if (GameFrameworkEntry.Cache.TryGetValue(url, out ResObject resObject))
            {
                return resObject.GetAsset<TextAsset>(gameObject);
            }

            if (manifestManager.TryGetPackageManifestWithAssetName(url, out ResourcePackageManifest manifest))
            {
                resObject = ResObject.Create(url);
            }

            if (resObject is null || resObject.IsSuccess() is false)
            {
                return default;
            }

            return resObject.GetAsset<TextAsset>(gameObject);
        }

        /// <summary>
        /// 获取文本资源
        /// </summary>
        /// <param name="url"></param>
        /// <param name="gameObject"></param>
        /// <returns></returns>
        /// <exception cref="NullReferenceException"></exception>
        public async UniTask<TextAsset> GetTextAssetAsync(string url, GameObject gameObject)
        {
            if (url.IsNullOrEmpty())
            {
                throw new NullReferenceException(nameof(url));
            }

            if (GameFrameworkEntry.Cache.TryGetValue(url, out ResObject resObject))
            {
                return resObject.GetAsset<TextAsset>(gameObject);
            }

            if (manifestManager.TryGetPackageManifestWithAssetName(url, out ResourcePackageManifest manifest))
            {
                resObject = await ResObject.CreateAsync(url);
            }
            else
            {
                if (url.StartsWith("http"))
                {
                    using (UnityWebRequest request = UnityWebRequest.Get(url))
                    {
                        await request.SendWebRequest();
                        if (request.result is UnityWebRequest.Result.Success)
                        {
                            resObject = ResObject.Create(new TextAsset(request.downloadHandler.text), url);
                            GameFrameworkEntry.Cache.SetCacheData(resObject);
                        }
                    }
                }
            }

            if (resObject is null || resObject.IsSuccess() is false)
            {
                return default;
            }

            return resObject.GetAsset<TextAsset>(gameObject);
        }

        /// <summary>
        /// 加载热更新代码
        /// </summary>
        /// <param name="dllName"></param>
        /// <param name="mode"></param>
        /// <returns></returns>
        /// <exception cref="NullReferenceException"></exception>
        /// <exception cref="FileNotFoundException"></exception>
        public async UniTask<Assembly> LoadGameAssembly(string dllName, CodeMode mode)
        {
            if (mode is CodeMode.Native || (ResConfig.instance.resMode == ResourceMode.Editor && Application.isEditor))
            {
                if (dllName.IsNullOrEmpty())
                {
                    throw new NullReferenceException(nameof(dllName));
                }

                Debug.Log("原生代码：" + dllName);
                return AppDomain.CurrentDomain.GetAssemblies().Where(x => x.GetName().Name.Equals(dllName)).FirstOrDefault();
            }

            string aotFileName = $"{dllName.ToLower()}_aot.bytes";
            string hotfixFile = $"{dllName.ToLower()}_hotfix.bytes";
            if (manifestManager.TryGetPackageVersion(aotFileName, out uint crc) is false || manifestManager.TryGetPackageVersion(hotfixFile, out crc) is false)
            {
                throw new FileNotFoundException();
            }

            byte[] bytes = await _disk.ReadAsync(aotFileName);
            Dictionary<string, byte[]> aotZipDict = await Zip.Decompress(bytes);
            foreach (var VARIABLE in aotZipDict)
            {
                if (RuntimeApi.LoadMetadataForAOTAssembly(VARIABLE.Value, HomologousImageMode.SuperSet) != LoadImageErrorCode.OK)
                {
                    Debug.LogError("加载AOT补元数据资源失败:" + VARIABLE.Key);
                    continue;
                }
            }

            bytes = await _disk.ReadAsync(hotfixFile);
            Dictionary<string, byte[]> dllZipDict = await Zip.Decompress(bytes);
            if (dllZipDict.TryGetValue(dllName + ".dll", out byte[] dllBytes) is false)
            {
                throw new NullReferenceException(dllName);
            }

            Debug.Log("加载热更代码:" + dllName + ".dll");
            return Assembly.Load(dllBytes);
        }
    }
}