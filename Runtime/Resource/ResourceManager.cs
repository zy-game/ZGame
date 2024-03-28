using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using HybridCLR;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using ZGame.Config;
using ZGame.VFS;
using ZGame.Game;
using ZGame.Networking;
using ZGame.UI;
using Object = UnityEngine.Object;

namespace ZGame.Resource
{
    /// <summary>
    /// 资源管理器
    /// </summary>
    public sealed class ResourceManager : GameFrameworkModule
    {
        PackageManifestManager PackageManifest { get; set; }

        public override void OnAwake(params object[] args)
        {
            PackageManifest = GameFrameworkFactory.Spawner<PackageManifestManager>();
            PackageManifest.OnAwake();
        }

        public override void Release()
        {
            PackageManifest.Release();
            PackageManifest = null;
        }

        /// <summary>
        /// 根据资源名查找对应的资源包
        /// </summary>
        /// <param name="assetName"></param>
        /// <returns></returns>
        public ResourcePackageManifest GetResourcePackageManifestWithAssetName(string assetName)
        {
            return PackageManifest.GetResourcePackageManifestWithAssetName(assetName);
        }

        /// <summary>
        /// 获取需要更新资源包列表
        /// </summary>
        /// <param name="packageName"></param>
        /// <returns></returns>
        public ResourcePackageManifest[] CheckNeedUpdatePackageList(string packageName)
        {
            return PackageManifest.CheckNeedUpdatePackageList(packageName).ToArray();
        }

        /// <summary>
        /// 加载资源
        /// </summary>
        /// <param name="path">资源路径</param>
        /// <returns>资源加载结果</returns>
        public ResObject LoadAsset(string path, string extension = "")
        {
            return ResObjectLoadingHelper.LoadAssetObjectSync(path, extension);
        }

        /// <summary>
        /// 加载资源
        /// </summary>
        /// <param name="path">资源路径</param>
        /// <returns>资源加载任务</returns>
        public UniTask<ResObject> LoadAssetAsync(string path, string extension = "")
        {
            return ResObjectLoadingHelper.LoadAssetObjectAsync(path, extension);
        }

        /// <summary>
        /// 预加载资源包列表
        /// </summary>
        /// <param name="packageName">资源列表名</param>
        /// <returns></returns>
        public async UniTask<Status> PreloadingResourcePackageList(string packageName)
        {
            if (packageName.IsNullOrEmpty())
            {
                throw new ArgumentNullException("config");
            }

            await PackageManifest.SetupPackageManifest(packageName);
#if !UNITY_WEBGL //在webgl 下不更新资源包列表
            if (await ResPackageUpdateHelper.UpdateResourcePackageList(packageName) is not Status.Success)
            {
                return Status.Fail;
            }
#endif
            UILoading.SetTitle(GameFrameworkEntry.Language.Query("正在加载资源信息..."));
            UILoading.SetProgress(0);
#if UNITY_EDITOR
            if (ResConfig.instance.resMode == ResourceMode.Editor)
            {
                return Status.Success;
            }
#endif
            List<ResourcePackageManifest> packageList = PackageManifest.GetResourcePackageAndDependencyList(packageName);
            return await ResPackageLoadingHelper.LoadingResourcePackageListAsync(packageList.ToArray());
        }

        /// <summary>
        /// 卸载资源包
        /// </summary>
        /// <param name="packageNameList"></param>
        public void UnloadResourcePackage(params string[] packageNameList)
        {
            ResPackageLoadingHelper.UnloadResourcePackage(packageNameList);
        }


        /// <summary>
        /// 加载场景
        /// </summary>
        /// <param name="path">场景路径</param>
        /// <param name="callback">场景加载进度回调</param>
        /// <param name="mode">场景加载模式</param>
        /// <returns></returns>
        public ResObject LoadSceneSync(string path, IProgress<float> callback, LoadSceneMode mode = LoadSceneMode.Single)
        {
            return SceneObjectLoadingHelper.LoadSceneSync(path, callback, mode);
        }

        /// <summary>
        /// 加载场景
        /// </summary>
        /// <param name="path">场景路径</param>
        /// <param name="callback">场景加载进度回调</param>
        /// <param name="mode">场景加载模式</param>
        /// <returns></returns>
        public UniTask<ResObject> LoadSceneAsync(string path, IProgress<float> callback, LoadSceneMode mode = LoadSceneMode.Single)
        {
            return SceneObjectLoadingHelper.LoadSceneAsync(path, callback, mode);
        }

        /// <summary>
        /// 加载预制体
        /// </summary>
        /// <param name="path">预制体路径</param>
        /// <returns></returns>
        public GameObject LoadGameObjectSync(string path)
        {
            ResObject resObject = LoadAsset(path);
            if (resObject.IsSuccess() is false)
            {
                Debug.LogError("加载资源失败：" + path);
                return default;
            }

            GameObject gameObject = (GameObject)GameObject.Instantiate(resObject.GetAsset<Object>(null));
            gameObject.RegisterGameObjectDestroyEvent(() => { resObject.Release(); });
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
        public GameObject LoadGameObjectSync(string path, GameObject parent, Vector3 pos, Vector3 rot, Vector3 scale)
        {
            GameObject gameObject = LoadGameObjectSync(path);
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
        public async UniTask<GameObject> LoadGameObjectAsync(string path)
        {
            ResObject resObject = await LoadAssetAsync(path);
            if (resObject.IsSuccess() is false)
            {
                Debug.LogError("加载资源失败：" + path);
                return default;
            }

            GameObject gameObject = (GameObject)GameObject.Instantiate(resObject.GetAsset<Object>(null));
            gameObject.RegisterGameObjectDestroyEvent(() => { resObject.Release(); });
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
        public async UniTask<GameObject> LoadGameObjectAsync(string path, GameObject parent, Vector3 pos, Vector3 rot, Vector3 scale)
        {
            GameObject gameObject = await LoadGameObjectAsync(path);
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
        public AudioClip LoadAudioClipSync(string path, GameObject gameObject)
        {
            ResObject resObject = LoadAsset(path);
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
        public async UniTask<AudioClip> LoadAudioClipAsync(string path, GameObject gameObject)
        {
            ResObject resObject = await LoadAssetAsync(path);
            if (resObject.IsSuccess() is false)
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
        public Texture2D LoadTexture2DSync(string path, GameObject gameObject)
        {
            ResObject resObject = LoadAsset(path);
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
        public async UniTask<Texture2D> LoadTexture2DAsync(string path, GameObject gameObject)
        {
            ResObject resObject = await LoadAssetAsync(path);
            if (resObject.IsSuccess() is false)
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
        public Sprite LoadSpriteSync(string path, GameObject gameObject)
        {
            ResObject resObject = LoadAsset(path);
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
        public async UniTask<Sprite> LoadSpriteAsync(string path, GameObject gameObject)
        {
            ResObject resObject = await LoadAssetAsync(path);
            if (resObject.IsSuccess() is false)
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
        public Material LoadMaterialSync(string path, GameObject gameObject)
        {
            ResObject resObject = LoadAsset(path);
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
        public async UniTask<Material> LoadMaterialAsync(string path, GameObject gameObject)
        {
            ResObject resObject = await LoadAssetAsync(path);
            if (resObject.IsSuccess() is false)
            {
                Debug.LogError("加载资源失败：" + path);
                return default;
            }

            return resObject.GetAsset<Material>(gameObject);
        }

        /// <summary>
        /// 卸载单个资源
        /// </summary>
        /// <param name="path"></param>
        public void UnloadAsset(string path)
        {
        }
    }
}