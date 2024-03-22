using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Cysharp.Threading.Tasks;
using HybridCLR;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using ZGame.Config;
using ZGame.FileSystem;
using ZGame.Game;
using ZGame.Networking;
using ZGame.Resource.Config;
using ZGame.UI;
using Object = UnityEngine.Object;

namespace ZGame.Resource
{
    /// <summary>
    /// 资源管理器
    /// </summary>
    public sealed class ResourceManager : GameFrameworkModule
    {
        internal ResPackageCache ResPackageCache { get; set; }
        internal ResObjectCache ResObjectCache { get; set; }
        internal PackageManifestManager PackageManifest { get; set; }

        public override void OnAwake()
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

        public override void Dispose()
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
        public async UniTask<bool> PreloadingResourcePackageList(string packageName)
        {
            if (packageName.IsNullOrEmpty())
            {
                throw new ArgumentNullException("config");
            }

            Extension.StartSample();
            await PackageManifest.SetupPackageManifest(packageName);
            bool state = await ResPackageCache.UpdateResourcePackageList(packageName);
            if (state is false)
            {
                return false;
            }

            UILoading.SetTitle(GameFrameworkEntry.Language.Query("正在加载资源信息..."));
            UILoading.SetProgress(0);
            List<ResourcePackageManifest> manifests = PackageManifest.GetResourcePackageAndDependencyList(packageName);
            if (manifests is null || manifests.Count == 0)
            {
                UILoading.SetTitle(GameFrameworkEntry.Language.Query("资源加载完成..."));
                UILoading.SetProgress(1);
                return true;
            }

            Debug.Log("加载资源：" + string.Join(",", manifests.Select(x => x.name)));
            await ResPackageCache.LoadingResourcePackageListAsync(manifests.ToArray());
            Debug.Log($"资源预加载完成，总计耗时：{Extension.GetSampleTime()}");
            return true;
        }

        /// <summary>
        /// 加载场景
        /// </summary>
        /// <param name="path"></param>
        /// <param name="mode"></param>
        /// <returns></returns>
        public Scene LoadSceneSync(string path, LoadSceneMode mode = LoadSceneMode.Single)
        {
            ResObject obj = LoadAsset(path);
            Scene scene = obj.GetAsset<Scene>();

            if (obj != null && scene.isLoaded)
            {
                return scene;
            }

            LoadSceneParameters parameters = new LoadSceneParameters(LoadSceneMode.Single);
#if UNITY_EDITOR
            if (ResConfig.instance.resMode == ResourceMode.Editor)
            {
                scene = UnityEditor.SceneManagement.EditorSceneManager.LoadSceneInPlayMode(path, parameters);
            }
#endif
            if (scene == null)
            {
                scene = SceneManager.LoadScene(Path.GetFileNameWithoutExtension(path), parameters);
            }

            return scene;
        }

        /// <summary>
        /// 加载场景
        /// </summary>
        /// <param name="path"></param>
        /// <param name="mode"></param>
        /// <returns></returns>
        public async UniTask<Scene> LoadSceneAsync(string path, LoadSceneMode mode = LoadSceneMode.Single)
        {
            ResObject obj = await LoadAssetAsync(path);
            Scene scene = obj.GetAsset<Scene>();
            if (obj != null && scene.isLoaded)
            {
                return scene;
            }

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
                operation = SceneManager.LoadSceneAsync(Path.GetFileNameWithoutExtension(path), parameters);
            }

            await operation.ToUniTask(UILoading.Show());
            scene = SceneManager.GetSceneAt(SceneManager.sceneCount - 1);
            UILoading.Hide();
            return scene;
        }

        /// <summary>
        /// 加载预制体
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public GameObject LoadGameObjectSync(string path)
        {
            ResObject resObject = LoadAsset(path);
            if (resObject.IsSuccess() is false)
            {
                Debug.LogError("加载资源失败：" + path);
                return default;
            }

            GameObject gameObject = (GameObject)GameObject.Instantiate(resObject.GetAsset<Object>());
            gameObject.RegisterGameObjectDestroyEvent(() => { resObject.Release(); });
            return gameObject;
        }

        /// <summary>
        /// 加载预制体
        /// </summary>
        /// <param name="path"></param>
        /// <param name="parent"></param>
        /// <param name="pos"></param>
        /// <param name="rot"></param>
        /// <param name="scale"></param>
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
        /// <param name="path"></param>
        /// <returns></returns>
        public async UniTask<GameObject> LoadGameObjectAsync(string path)
        {
            ResObject resObject = await LoadAssetAsync(path);
            if (resObject.IsSuccess() is false)
            {
                Debug.LogError("加载资源失败：" + path);
                return default;
            }

            GameObject gameObject = (GameObject)GameObject.Instantiate(resObject.GetAsset<Object>());
            gameObject.RegisterGameObjectDestroyEvent(() => { resObject.Release(); });
            return gameObject;
        }

        /// <summary>
        /// 加载预制体
        /// </summary>
        /// <param name="path"></param>
        /// <param name="parent"></param>
        /// <param name="pos"></param>
        /// <param name="rot"></param>
        /// <param name="scale"></param>
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
        /// 加载精灵图片
        /// </summary>
        /// <param name="image"></param>
        /// <param name="path"></param>
        public void LoadSpriteSync(Image image, string path)
        {
            if (image == null)
            {
                return;
            }

            ResObject resObject = LoadAsset(path);
            if (resObject.IsSuccess() is false)
            {
                return;
            }

            image.sprite = resObject.GetAsset<Sprite>();
        }

        /// <summary>
        /// 加载精灵图片
        /// </summary>
        /// <param name="image"></param>
        /// <param name="path"></param>
        public async void LoadSpriteAsync(Image image, string path)
        {
            if (image == null)
            {
                return;
            }

            ResObject resObject = await LoadAssetAsync(path);
            if (resObject is null || resObject.IsSuccess() is false)
            {
                return;
            }

            image.sprite = resObject.GetAsset<Sprite>();
        }

        /// <summary>
        /// 加载图片
        /// </summary>
        /// <param name="image"></param>
        /// <param name="path"></param>
        public void LoadTexture2DSync(RawImage image, string path)
        {
            if (image == null)
            {
                return;
            }

            ResObject resObject = LoadAsset(path);
            if (resObject.IsSuccess() is false)
            {
                return;
            }

            image.texture = resObject.GetAsset<Texture2D>();
        }

        /// <summary>
        /// 加载图片
        /// </summary>
        /// <param name="image"></param>
        /// <param name="path"></param>
        public async void LoadTexture2DAsync(RawImage image, string path)
        {
            if (image == null)
            {
                return;
            }

            ResObject resObject = await LoadAssetAsync(path);
            if (resObject is null || resObject.IsSuccess() is false)
            {
                return;
            }

            image.texture = resObject.GetAsset<Texture2D>();
        }

        /// <summary>
        /// 设置材质球图片
        /// </summary>
        /// <param name="material"></param>
        /// <param name="propertyName"></param>
        /// <param name="gameObject"></param>
        /// <param name="path"></param>
        public void LoadMaterialTexture2DSync(Material material, string propertyName, GameObject gameObject, string path)
        {
            if (material == null || gameObject == null)
            {
                return;
            }

            ResObject resObject = LoadAsset(path);
            if (resObject.IsSuccess() is false)
            {
                return;
            }

            material.mainTexture = resObject.GetAsset<Texture2D>();
        }

        /// <summary>
        /// 设置材质球图片
        /// </summary>
        /// <param name="material"></param>
        /// <param name="propertyName"></param>
        /// <param name="gameObject"></param>
        /// <param name="path"></param>
        public async void LoadMaterialTexture2DAsync(Material material, string propertyName, GameObject gameObject, string path)
        {
            if (material == null || gameObject == null)
            {
                return;
            }

            ResObject resObject = await LoadAssetAsync(path);
            if (resObject is null || resObject.IsSuccess() is false)
            {
                return;
            }

            material.mainTexture = resObject.GetAsset<Texture2D>();
        }

        /// <summary>
        /// 设置材质球
        /// </summary>
        /// <param name="renderer"></param>
        /// <param name="path"></param>
        public void LoadRenderMaterialSync(Renderer renderer, string path)
        {
            if (renderer == null)
            {
                return;
            }

            ResObject resObject = LoadAsset(path);
            if (resObject.IsSuccess() is false)
            {
                return;
            }

            renderer.sharedMaterial = resObject.GetAsset<Material>();
        }

        /// <summary>
        /// 设置材质球
        /// </summary>
        /// <param name="renderer"></param>
        /// <param name="path"></param>
        public async void LoadRenderMaterialAsync(Renderer renderer, string path)
        {
            if (renderer == null)
            {
                return;
            }

            ResObject resObject = await LoadAssetAsync(path);
            if (resObject is null || resObject.IsSuccess() is false)
            {
                return;
            }

            renderer.sharedMaterial = resObject.GetAsset<Material>();
        }

        public async UniTask LoadGameAssembly(GameConfig config)
        {
            Assembly assembly = default;
            string dllName = Path.GetFileNameWithoutExtension(config.path);
            if (config.mode is CodeMode.Native || (ResConfig.instance.resMode == ResourceMode.Editor && Application.isEditor))
            {
                Debug.Log("原生代码：" + dllName);
                if (dllName.IsNullOrEmpty())
                {
                    throw new NullReferenceException(nameof(dllName));
                }

                assembly = AppDomain.CurrentDomain.GetAssemblies().Where(x => x.GetName().Name.Equals(dllName)).FirstOrDefault();
            }
            else
            {
                Dictionary<string, byte[]> aotZipDict = await Zip.Decompress(GameFrameworkEntry.VFS.Read($"{dllName.ToLower()}_aot.bytes"));
                foreach (var VARIABLE in aotZipDict)
                {
                    if (RuntimeApi.LoadMetadataForAOTAssembly(VARIABLE.Value, HomologousImageMode.SuperSet) != LoadImageErrorCode.OK)
                    {
                        Debug.LogError("加载AOT补元数据资源失败:" + VARIABLE.Key);
                        continue;
                    }

                    Debug.Log("加载补充元数据成功：" + VARIABLE.Key);
                }

                Dictionary<string, byte[]> dllZipDict = await Zip.Decompress(GameFrameworkEntry.VFS.Read($"{dllName.ToLower()}_hotfix.bytes"));
                if (dllZipDict.TryGetValue(dllName + ".dll", out byte[] dllBytes) is false)
                {
                    throw new NullReferenceException(dllName);
                }

                Debug.Log("加载热更代码:" + dllName + ".dll Lenght:" + dllBytes.Length);
                assembly = Assembly.Load(dllBytes);
            }

            if (assembly is null)
            {
                GameFrameworkEntry.Logger.LogError("加载DLL失败");
                return;
            }

            Type entryType = assembly.GetAllSubClasses<SubGameStartup>().FirstOrDefault();
            if (entryType is null)
            {
                throw new EntryPointNotFoundException();
            }

            SubGameStartup startup = Activator.CreateInstance(entryType) as SubGameStartup;
            if (startup is null)
            {
                Debug.LogError("加载入口失败");
                return;
            }

            Debug.Log("Entry SubGame:" + startup);
            ResultStatus status = await startup.OnEntry();
            if (status is not ResultStatus.Success)
            {
                Debug.LogError("进入游戏失败");
                return;
            }

            UILoading.Hide();
        }
    }
}