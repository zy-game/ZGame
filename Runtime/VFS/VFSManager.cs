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
        private float refreshTime;
        private const string EDITOR_RESOURCES_PACKAGE = "EDITOR_RESOURCES_PACKAGE";
        private const string NETWORK_RESOURCES_PACKAGE = "NETWORK_RESOURCES_PACKAGE";
        private const string INTERNAL_RESOURCES_PACKAGE = "INTERNAL_RESOURCES_PACKAGE";

        private ResourcePackageManifestManager manifestManager;

        public override void OnAwake(params object[] args)
        {
            refreshTime = Time.realtimeSinceStartup;
            manifestManager = new ResourcePackageManifestManager();
            _disk = NFSManager.OpenOrCreateDisk(GameConfig.instance.title + " virtual data.disk");
        }

        public override void Release()
        {
            GameFrameworkFactory.Release(manifestManager);
            GameFrameworkFactory.Release(_disk);
        }
#if UNITY_EDITOR
        public override void OnGUI()
        {
            ResPackage.OnDrawingGUI();
            ResObject.OnDrawingGUI();
        }
#endif

        public override void Update()
        {
            if (Time.realtimeSinceStartup - refreshTime < ResConfig.instance.timeout)
            {
                return;
            }

            refreshTime = Time.realtimeSinceStartup;
            ResPackage.ReleaseUnusedRefObject();
            ResPackage.CheckUnusedRefObject();
            ResObject.ReleaseUnusedRefObject();
            ResObject.CheckUnusedRefObject();
        }


        // internal bool TryGetResPackage(string name, out ResPackage package)
        // {
        //     package = packages.Find(x => x.name == name);
        //     if (package is null)
        //     {
        //         package = packageCache.Find(x => x.name == name);
        //         if (package is not null)
        //         {
        //             packageCache.Remove(package);
        //             packages.Add(package);
        //         }
        //     }
        //
        //     return package is not null;
        // }
        //
        // internal bool TryGetResObject(string name, out ResObject resObject)
        // {
        //     resObject = resObjects.Find(x => x.name == name);
        //     if (resObject is null)
        //     {
        //         resObject = resObjectCache.Find(x => x.name == name);
        //         if (resObject is not null)
        //         {
        //             resObjectCache.Remove(resObject);
        //             resObjects.Add(resObject);
        //         }
        //     }
        //
        //     return resObject is not null;
        // }

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
#if !UNITY_WEBGL
            if (await ResPackage.UpdateResourcePackageList(manifestManager.GetUpdateResourcePackageList(packageName).ToArray()) is not Status.Success)
            {
                return Status.Fail;
            }
#endif
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

        public ResObject GetAsset(string path)
        {
            return ResObject.LoadResObjectSync(path);
        }

        public async UniTask<ResObject> GetAssetAsync(string path)
        {
            if (path.IsNullOrEmpty())
            {
                return ResObject.DEFAULT;
            }

            return await ResObject.LoadResObjectAsync(path);
        }


        /// <summary>
        /// 加载预制体
        /// </summary>
        /// <param name="path">预制体路径</param>
        /// <returns></returns>
        public GameObject GetGameObjectSync(string path)
        {
            ResObject resObject = ResObject.LoadResObjectSync(path);
            if (resObject.IsSuccess() is false)
            {
                GameFrameworkEntry.Logger.LogError("加载资源失败：" + path);
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
            ResObject resObject = await ResObject.LoadResObjectAsync(path);
            if (resObject.IsSuccess() is false)
            {
                GameFrameworkEntry.Logger.LogError("加载资源失败：" + path);
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

        public async UniTask<ResObject> GetAudioStreamingAssetAsync(string path, AudioType type)
        {
            if (ResObject.TryGetValue(path, out ResObject resObject))
            {
                return resObject;
            }

            using (UnityWebRequest request = UnityWebRequestMultimedia.GetAudioClip(path, type))
            {
                await request.SendWebRequest();
                if (request.isNetworkError || request.isHttpError)
                {
                    GameFrameworkEntry.Logger.LogError("加载资源失败：" + path);
                    return ResObject.DEFAULT;
                }

                AudioClip clip = DownloadHandlerAudioClip.GetContent(request);
                clip.name = path;
                resObject = ResObject.Create(ResPackage.DEFAULT, clip, path);
                Debug.Log("audio clip:" + path);
            }

            return resObject;
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
                return ResObject.DEFAULT;
            }

            if (ResObject.TryGetValue(path, out ResObject sceneObject))
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

                if (ResPackage.TryGetValue(manifest.name, out package) is false)
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
            sceneObject = ResObject.Create(package, scene, path);
            UILoading.Hide();
            return sceneObject;
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

                GameFrameworkEntry.Logger.Log("原生代码：" + dllName);
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
                    GameFrameworkEntry.Logger.LogError("加载AOT补元数据资源失败:" + VARIABLE.Key);
                    continue;
                }
            }

            bytes = await _disk.ReadAsync(hotfixFile);
            Dictionary<string, byte[]> dllZipDict = await Zip.Decompress(bytes);
            if (dllZipDict.TryGetValue(dllName + ".dll", out byte[] dllBytes) is false)
            {
                throw new NullReferenceException(dllName);
            }

            GameFrameworkEntry.Logger.Log("加载热更代码:" + dllName + ".dll");
            return Assembly.Load(dllBytes);
        }
    }
}