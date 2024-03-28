using System;
using System.IO;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using ZGame.UI;

namespace ZGame.Resource
{
    class SceneObjectLoadingHelper
    {
        private static readonly ResObject EMPTY_OBJECT = new();

        public static ResObject LoadSceneSync(string path, IProgress<float> callback, LoadSceneMode mode = LoadSceneMode.Single)
        {
            Scene scene = default;
            ResObject sceneObject = default;
            ResourcePackageManifest manifest = GameFrameworkEntry.Resource.GetResourcePackageManifestWithAssetName(path);
            if (manifest is null)
            {
                Debug.Log("没有找到资源信息配置:" + path);
                return EMPTY_OBJECT;
            }

            if (GameFrameworkEntry.CacheObject.TryGetValue(manifest.name, out ResPackage _handle) is false)
            {
                return EMPTY_OBJECT;
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

            GameFrameworkEntry.CacheObject.SetCacheData(sceneObject = ResObject.Create(_handle, scene, path));
            return sceneObject;
        }

        public static async UniTask<ResObject> LoadSceneAsync(string path, IProgress<float> callback, LoadSceneMode mode = LoadSceneMode.Single)
        {
            Scene scene = default;
            ResObject sceneObject = default;
            AsyncOperation operation = default;
            ResourcePackageManifest manifest = GameFrameworkEntry.Resource.GetResourcePackageManifestWithAssetName(path);
            if (manifest is null)
            {
                Debug.Log("没有找到资源信息配置:" + path);
                return EMPTY_OBJECT;
            }

            if (GameFrameworkEntry.CacheObject.TryGetValue(manifest.name, out ResPackage _handle) is false)
            {
                await ResPackageLoadingHelper.LoadingResourcePackageListAsync(manifest);
                return await LoadSceneAsync(path, callback, mode);
            }

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
            GameFrameworkEntry.CacheObject.SetCacheData(sceneObject = ResObject.Create(_handle, scene, path));
            UILoading.Hide();
            return sceneObject;
        }
    }
}