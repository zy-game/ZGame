using System.Collections.Generic;
using System.IO;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;
using ZGame.Resource;
using ZGame.Window;

namespace ZGame
{
    public static partial class Extension
    {
        public static Scene LoadScene(this ResHandle resHandle)
        {
            Scene scene = resHandle.Require<Scene>();
            if (scene != null && scene.isLoaded)
            {
                return scene;
            }

            LoadSceneParameters parameters = new LoadSceneParameters(LoadSceneMode.Single);
#if UNITY_EDITOR
            if (GlobalConfig.current.runtime == RuntimeMode.Editor)
            {
                scene = UnityEditor.SceneManagement.EditorSceneManager.LoadSceneInPlayMode(resHandle.path, parameters);
            }

#endif
            if (scene == null)
            {
                scene = SceneManager.LoadScene(Path.GetFileNameWithoutExtension(resHandle.path), parameters);
            }

            return scene;
        }

        public static async UniTask<Scene> LoadSceneAsync(this ResHandle resHandle, ILoadingHandle loadingHandle = null)
        {
            Scene scene = resHandle.Require<Scene>();
            if (scene != null && scene.isLoaded)
            {
                return scene;
            }

            AsyncOperation operation = default;
            LoadSceneParameters parameters = new LoadSceneParameters(LoadSceneMode.Single);
#if UNITY_EDITOR
            if (GlobalConfig.current.runtime == RuntimeMode.Editor)
            {
                operation = UnityEditor.SceneManagement.EditorSceneManager.LoadSceneAsyncInPlayMode(resHandle.path, parameters);
            }
#endif
            if (operation == null)
            {
                operation = SceneManager.LoadSceneAsync(Path.GetFileNameWithoutExtension(resHandle.path), parameters);
            }

            await operation.ToUniTask(loadingHandle);
            scene = SceneManager.GetSceneAt(SceneManager.sceneCount - 1);
            
            return scene;
        }
        
        


        public static GameObject Instantiate(this ResHandle resObject)
        {
            if (resObject is null)
            {
                return default;
            }

            GameObject gameObject = (GameObject)GameObject.Instantiate(resObject.Require<GameObject>());
            gameObject.OnDestroyEventCallback(() => { ResourceManager.instance.ReleaseAsset(resObject); });
            return gameObject;
        }

        public static GameObject Instantiate(this ResHandle loadResourceObjectResult, GameObject parent, Vector3 pos, Vector3 rot, Vector3 scale)
        {
            GameObject gameObject = loadResourceObjectResult.Instantiate();
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

        public static void SetSprite(this ResHandle resObject, GameObject gameObject)
        {
            if (resObject is null)
            {
                return;
            }

            Image image = gameObject.GetComponent<Image>();
            if (image == null)
            {
                return;
            }

            image.sprite = resObject.Require<Sprite>();
            gameObject.OnDestroyEventCallback(() => { ResourceManager.instance.ReleaseAsset(resObject); });
        }

        public static void SetRawImage(this ResHandle resObject, GameObject gameObject)
        {
            if (resObject is null)
            {
                return;
            }

            RawImage image = gameObject.GetComponent<RawImage>();
            if (image == null)
            {
                return;
            }

            image.texture = resObject.Require<Texture2D>();
            gameObject.OnDestroyEventCallback(() => { ResourceManager.instance.ReleaseAsset(resObject); });
        }

        public static void SetSound(this ResHandle resObject, GameObject gameObject)
        {
            if (resObject is null)
            {
                return;
            }

            AudioSource component = gameObject.GetComponent<AudioSource>();
            if (component == null)
            {
                return;
            }

            component.clip = resObject.Require<AudioClip>();
            gameObject.OnDestroyEventCallback(() => { ResourceManager.instance.ReleaseAsset(resObject); });
        }

        public static void SetVideoClip(this ResHandle resObject, GameObject gameObject)
        {
            if (resObject is null)
            {
                return;
            }

            VideoPlayer component = gameObject.GetComponent<VideoPlayer>();
            if (component == null)
            {
                return;
            }

            component.clip = resObject.Require<VideoClip>();
            gameObject.OnDestroyEventCallback(() => { ResourceManager.instance.ReleaseAsset(resObject); });
        }
    }
}