using System;
using System.IO;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;
using ZGame.Window;

namespace ZGame.Resource
{
    public sealed class ResHandle : IDisposable
    {
        private int count;
        private object obj;
        private ResPackageHandle parent;
        public int refCount => count;
        public string path { get; private set; }


        public static void OnCreate(ResPackageHandle parent, object obj, string path)
        {
        }

        public ResHandle(ResPackageHandle parent, object obj, string path)
        {
            this.obj = obj;
            this.path = path;
            this.parent = parent;
        }

        public T Get<T>()
        {
            count++;
            this.parent.AddRef();
            return obj == null ? default(T) : (T)obj;
        }

        public void Release()
        {
            count--;
            this.parent.RemoveRef();
        }

        public void Dispose()
        {
            obj = null;
            parent = null;
            count = 0;
            path = String.Empty;
        }

        public Scene OpenScene()
        {
            Scene scene = Get<Scene>();

            if (obj != null && scene.isLoaded)
            {
                return scene;
            }

            LoadSceneParameters parameters = new LoadSceneParameters(LoadSceneMode.Single);
#if UNITY_EDITOR
            if (GlobalConfig.instance.resConfig.resMode == ResourceMode.Editor)
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

        public async UniTask<Scene> OpenSceneAsync(ILoadingHandle loadingHandle = null)
        {
            Scene scene = Get<Scene>();

            if (obj != null && scene.isLoaded)
            {
                return scene;
            }

            AsyncOperation operation = default;
            LoadSceneParameters parameters = new LoadSceneParameters(LoadSceneMode.Single);
#if UNITY_EDITOR
            if (GlobalConfig.instance.resConfig.resMode == ResourceMode.Editor)
            {
                operation = UnityEditor.SceneManagement.EditorSceneManager.LoadSceneAsyncInPlayMode(path, parameters);
            }
#endif
            if (operation == null)
            {
                operation = SceneManager.LoadSceneAsync(Path.GetFileNameWithoutExtension(path), parameters);
            }

            await operation.ToUniTask(loadingHandle);
            scene = SceneManager.GetSceneAt(SceneManager.sceneCount - 1);
            return scene;
        }


        public GameObject Instantiate()
        {
            if (obj is null)
            {
                return default;
            }

            GameObject gameObject = (GameObject)GameObject.Instantiate(Get<GameObject>());
            ListenerDestroyEvent(gameObject);
            return gameObject;
        }

        public GameObject Instantiate(GameObject parent, Vector3 pos, Vector3 rot, Vector3 scale)
        {
            if (obj is null)
            {
                return default;
            }

            GameObject gameObject = Instantiate();
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

        public void Setup<T>(GameObject gameObject) where T : Component
        {
            if (obj is null)
            {
                return;
            }

            Component component = gameObject.GetComponent<T>();
            switch (component)
            {
                case Image image:
                    image.sprite = Get<Sprite>();
                    break;
                case RawImage rawImage:
                    rawImage.texture = Get<Texture2D>();
                    break;
                case AudioSource audioSource:
                    audioSource.clip = Get<AudioClip>();
                    break;
                case VideoPlayer videoPlayer:
                    videoPlayer.clip = Get<VideoClip>();
                    break;
                case TMP_InputField inputField:
                    switch (obj)
                    {
                        case TextAsset textAsset:
                            inputField.text = textAsset.text;
                            break;
                        case TMP_FontAsset fontAsset:
                            inputField.fontAsset = fontAsset;
                            break;
                    }

                    break;
                case TMP_Text tmpText:
                    switch (obj)
                    {
                        case TextAsset textAsset:
                            tmpText.text = textAsset.text;
                            break;
                        case TMP_FontAsset fontAsset:
                            tmpText.font = fontAsset;
                            break;
                    }

                    break;
            }

            ListenerDestroyEvent(gameObject);
        }

        private void ListenerDestroyEvent(GameObject gameObject)
        {
            if (gameObject == null)
            {
                return;
            }

            ZGame.BevaviourScriptable bevaviour = gameObject.GetComponent<ZGame.BevaviourScriptable>();
            if (bevaviour == null)
            {
                bevaviour = gameObject.AddComponent<ZGame.BevaviourScriptable>();
            }

            bevaviour.onDestroy.AddListener(() => { ResourceManager.instance.ReleaseAsset(this); });
        }
    }
}