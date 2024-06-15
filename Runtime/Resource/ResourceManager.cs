using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using ZGame.Language;
using ZGame.UI;
using Object = UnityEngine.Object;

namespace ZGame.Resource
{
    /// <summary>
    /// 资源管理器
    /// </summary>
    public class ResourceManager : GameManager
    {
        private float lastCheckUnuseResourceTime;

        public override void OnAwake(params object[] args)
        {
            lastCheckUnuseResourceTime = Time.realtimeSinceStartup;
        }

        public override void Release()
        {
        }
#if UNITY_EDITOR
        protected override void OnGUI()
        {
            ResPackage.OnDrawingGUI();
            ResObject.OnDrawingGUI();
        }
#endif

        protected override void Update()
        {
            if (Time.realtimeSinceStartup - lastCheckUnuseResourceTime < AppCore.timeout)
            {
                return;
            }

            UnloadUnuseResources();
        }


        /// <summary>
        /// 加载资源包
        /// </summary>
        /// <param name="packageName"></param>
        /// <returns></returns>
        public async UniTask<Status> LoadingResourcePackageAsync(string packageName)
        {
            if (await AppCore.Manifest.SetPackageManifest(packageName) is not Status.Success)
            {
                return Status.Fail;
            }

            var manifests = AppCore.Manifest.GetResourcePackageAndDependencyList(packageName);
#if !UNITY_WEBGL
            var updateList = manifests.Where(x => AppCore.File.Exist(x.name, x.version) is false);
            if (updateList.Count() > 0)
            {
                if (await ResPackage.UpdatePackageAsync(updateList.ToArray()) is not Status.Success)
                {
                    return Status.Fail;
                }
            }
#endif
            UILoading.SetTitle(AppCore.Language.Query(LanguageCode.LoadingPackageBundle));
            if (await ResPackage.LoadBundleAsync(manifests.ToArray()) is not Status.Success)
            {
                return Status.Fail;
            }

            UILoading.SetTitle(AppCore.Language.Query(LanguageCode.PackageLoadingComplete));
            UILoading.SetProgress(1);
            return Status.Success;
        }

        /// <summary>
        /// 卸载未使用的资源对象和资源包
        /// </summary>
        /// <param name="packageNameList"></param>
        public void UnloadUnuseResources()
        {
            lastCheckUnuseResourceTime = Time.realtimeSinceStartup;
            ResPackage.ReleaseUnusedRefObject();
            ResPackage.CheckUnusedRefObject();
            ResObject.ReleaseUnusedRefObject();
            ResObject.CheckUnusedRefObject();
        }

        /// <summary>
        /// 卸载资源对象
        /// </summary>
        /// <param name="PathList"></param>
        public void Unload(params string[] PathList)
        {
            foreach (var VARIABLE in PathList)
            {
                if (ResObject.TryGetValue(VARIABLE, out var resObject))
                {
                    ResObject.Unload(resObject);
                }
            }
        }

        /// <summary>
        /// 卸载资源对象
        /// </summary>
        /// <param name="PathList"></param>
        public void Unload(params ResObject[] PathList)
        {
            Unload(PathList.Select(x => x.name).ToArray());
        }

        /// <summary>
        /// 卸载资源包
        /// </summary>
        /// <param name="packageNameList"></param>
        public void UnloadResPackage(params string[] packageNameList)
        {
            foreach (var VARIABLE in packageNameList)
            {
                List<ResourcePackageManifest> manifests = AppCore.Manifest.GetResourcePackageAndDependencyList(VARIABLE);
                if (manifests is null || manifests.Count == 0)
                {
                    return;
                }

                manifests.ForEach(x => ResPackage.Unload(x.name));
            }
        }

        /// <summary>
        /// 加载资源
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public ResObject LoadAsset<T>(string path) where T : Object
        {
            if (path.IsNullOrEmpty())
            {
                return ResObject.DEFAULT;
            }

            AppCore.Logger.Log($"{nameof(LoadAsset)}: {path}");
            return ResObject.LoadObject<T>(path);
        }

        /// <summary>
        /// 加载资源
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public async UniTask<ResObject> LoadAssetAsync<T>(string path, CancellationToken cancellationToken = default) where T : Object
        {
            if (path.IsNullOrEmpty())
            {
                return ResObject.DEFAULT;
            }

            AppCore.Logger.Log($"{nameof(LoadAssetAsync)}: {path}");
            return await ResObject.LoadObjectAsync<T>(path).AttachExternalCancellation(cancellationToken);
        }

        /// <summary>
        /// 获取网络资源
        /// </summary>
        /// <param name="path"></param>
        /// <param name="type"></param>
        /// <param name="assetName"></param>
        /// <returns></returns>
        public async UniTask<ResObject> LoadStreamingAssetAsync(string path, StreamingAssetType type, string assetName = "", CancellationToken cancellationToken = default)
        {
            if (path.IsNullOrEmpty())
            {
                return ResObject.DEFAULT;
            }

            AppCore.Logger.Log($"{nameof(LoadStreamingAssetAsync)}: {path}");
            return await ResObject.LoadStreamingObjectAsync(path, type, assetName).AttachExternalCancellation(cancellationToken);
        }

        /// <summary>
        /// 加载场景
        /// </summary>
        /// <param name="path">场景路径</param>
        /// <param name="callback">场景加载进度回调</param>
        /// <param name="mode">场景加载模式</param>
        /// <returns></returns>
        public async UniTask<ResObject> LoadSceneAsync(string path, IProgress<float> callback, CancellationToken cancellationToken = default, LoadSceneMode mode = LoadSceneMode.Single)
        {
            if (path.IsNullOrEmpty())
            {
                return ResObject.DEFAULT;
            }

            AppCore.Logger.Log($"{nameof(LoadSceneAsync)}: {path}");
            return await ResObject.LoadSceneAsync(path, mode, callback).AttachExternalCancellation(cancellationToken);
        }
    }
}