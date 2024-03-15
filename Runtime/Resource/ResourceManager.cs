using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using ZGame.Config;
using ZGame.FileSystem;
using ZGame.Game;
using ZGame.Module;
using ZGame.Networking;
using ZGame.Resource.Config;
using ZGame.UI;

namespace ZGame.Resource
{
    /// <summary>
    /// 资源管理器
    /// </summary>
    public sealed class ResourceManager : IModule
    {
        internal ResPackageCache ResPackageCache { get; set; }
        internal ResObjectCache ResObjectCache { get; set; }
        internal PackageManifestManager PackageManifest { get; set; }

        public void OnAwake()
        {
            SceneManager.sceneUnloaded += UnloadScene;
            ResPackageCache = new ResPackageCache();
            ResPackageCache.OnAwake();
            ResObjectCache = new ResObjectCache();
            ResObjectCache.OnAwake();
            PackageManifest = new PackageManifestManager();
            PackageManifest.OnAwake();
        }

        private void UnloadScene(Scene scene)
        {
        }

        public void Dispose()
        {
            ResPackageCache.Dispose();
            ResObjectCache.Dispose();
            PackageManifest.Dispose();
            ResPackageCache = null;
            ResObjectCache = null;
            PackageManifest = null;
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// 加载资源
        /// </summary>
        /// <param name="path">资源路径</param>
        /// <returns>资源加载结果</returns>
        public ResObject LoadAsset(string path, string extension = "")
        {
            return ResObjectCache.LoadSync(path, extension);
        }

        /// <summary>
        /// 加载资源
        /// </summary>
        /// <param name="path">资源路径</param>
        /// <returns>资源加载任务</returns>
        public async UniTask<ResObject> LoadAssetAsync(string path, string extension = "")
        {
            return await ResObjectCache.LoadAsync(path, extension);
        }

        /// <summary>
        /// 预加载资源包列表
        /// </summary>
        /// <param name="progressCallback"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public async UniTask<bool> PreloadingResourcePackageList(GameConfig config)
        {
            if (config is null)
            {
                throw new ArgumentNullException("config");
            }

            Extension.StartSample();
            await PackageManifest.SetupPackageManifest(BasicConfig.instance.curGame.module);
            bool state = await ResPackageCache.UpdateResourcePackageList(config);
            if (state is false)
            {
                return false;
            }

            UILoading.SetTitle(WorkApi.Language.Query("正在加载资源信息..."));
            UILoading.SetProgress(0);
            List<ResourcePackageManifest> manifests = PackageManifest.GetResourcePackageAndDependencyList(config.module);
            if (manifests is null || manifests.Count == 0)
            {
                UILoading.SetTitle(WorkApi.Language.Query("资源加载完成..."));
                UILoading.SetProgress(1);
                return true;
            }

            Debug.Log("加载资源：" + string.Join(",", manifests.Select(x => x.name)));
            await ResPackageCache.LoadingResourcePackageListAsync(manifests.ToArray());
            Debug.Log($"资源预加载完成，总计耗时：{Extension.GetSampleTime()}");
            return true;
        }

        public Scene OpenSceneSync(string path, LoadSceneMode mode = LoadSceneMode.Single)
        {
            return default;
        }


        public async UniTask<Scene> OpenSceneAsync(string path, LoadSceneMode mode = LoadSceneMode.Single)
        {
            return default;
        }


        public GameObject InstantiateSync(string path)
        {
            ResObject resObject = LoadAsset(path);
            if (resObject is null)
            {
                Debug.LogError("加载资源失败：" + path);
                return default;
            }

            GameObject gameObject = (GameObject)GameObject.Instantiate(resObject.Asset);
            return default;
        }

        public GameObject InstantiateAsync(string path)
        {
            return default;
        }

        public GameObject InstantiateSync(string path, GameObject parent, Vector3 pos, Vector3 rot, Vector3 scale)
        {
            GameObject gameObject = InstantiateSync(path);
            if (gameObject != null)
            {
                if (parent != null)
                {
                    gameObject.transform.SetParent(parent.transform);
                }

                gameObject.transform.position = pos;
                gameObject.transform.rotation = Quaternion.Euler(rot);
                gameObject.transform.localScale = scale;
            }

            return gameObject;
        }

        public GameObject InstantiateAsync(string path, GameObject parent, Vector3 pos, Vector3 rot, Vector3 scale)
        {
            GameObject gameObject = InstantiateAsync(path);
            if (gameObject != null)
            {
                if (parent != null)
                {
                    gameObject.transform.SetParent(parent.transform);
                }

                gameObject.transform.position = pos;
                gameObject.transform.rotation = Quaternion.Euler(rot);
                gameObject.transform.localScale = scale;
            }

            return gameObject;
        }

        public void SetSpriteSync(Image image, string path)
        {
        }

        public void SetSpriteAsync(Image image, string path)
        {
        }

        public void SetTexture2DSync(RawImage image, string path)
        {
        }

        public void SetTexture2DAsync(RawImage image, string path)
        {
        }

        public void SetMaterialTexture2DSync(Material material, string propertyName, GameObject gameObject, string path)
        {
        }

        public void SetMaterialTexture2DAsync(Material material, string propertyName, GameObject gameObject, string path)
        {
        }

        public void SetRenderMaterialSync(Renderer renderer, string path)
        {
        }

        public void SetRenderMaterialAsync(Renderer renderer, string path)
        {
        }

        public void SetGraphicMaterialSync(MaskableGraphic graphic, string path)
        {
        }

        public void SetGraphicMaterialAsync(MaskableGraphic graphic, string path)
        {
        }
    }
}