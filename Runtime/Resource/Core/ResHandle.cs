using System;
using System.IO;
using Cysharp.Threading.Tasks;
using TMPro;
using UI;
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


        public static ResHandle OnCreate(ResPackageHandle parent, object obj, string path)
        {
            return new ResHandle()
            {
                obj = obj,
                path = path,
                parent = parent
            };
        }

        public bool EnsureLoadSuccess()
        {
            if (obj != null)
            {
                return true;
            }

            return false;
        }

        public T Get<T>(GameObject gameObject)
        {
            count++;
            this.parent.AddRef();
            ListenerDestroyEvent(gameObject);
            return obj == null ? default(T) : (T)obj;
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
            Scene scene = Get<Scene>(default);

            if (obj != null && scene.isLoaded)
            {
                return scene;
            }

            LoadSceneParameters parameters = new LoadSceneParameters(LoadSceneMode.Single);
#if UNITY_EDITOR
            if (GlobalConfig.instance.curEntry.resMode == ResourceMode.Editor)
            {
                scene = UnityEditor.SceneManagement.EditorSceneManager.LoadSceneInPlayMode(path, parameters);
            }
#endif
            if (scene == null)
            {
                scene = SceneManager.LoadScene(Path.GetFileNameWithoutExtension(path), parameters);
            }

            SceneManager.sceneUnloaded += UnloadScene;
            return scene;
        }

        private void UnloadScene(Scene scene)
        {
            if (scene.path.Equals(path) is false)
            {
                return;
            }

            this.parent.RemoveRef();
            SceneManager.sceneUnloaded -= UnloadScene;
        }

        public async UniTask<Scene> OpenSceneAsync()
        {
            Scene scene = Get<Scene>(default);

            if (obj != null && scene.isLoaded)
            {
                return scene;
            }

            AsyncOperation operation = default;
            LoadSceneParameters parameters = new LoadSceneParameters(LoadSceneMode.Single);
#if UNITY_EDITOR
            if (GlobalConfig.instance.curEntry.resMode == ResourceMode.Editor)
            {
                operation = UnityEditor.SceneManagement.EditorSceneManager.LoadSceneAsyncInPlayMode(path, parameters);
            }
#endif
            if (operation == null)
            {
                operation = SceneManager.LoadSceneAsync(Path.GetFileNameWithoutExtension(path), parameters);
            }

            ILoading handler = (ILoading)UIManager.instance.TryOpenWindow(typeof(ILoading));
            await operation.ToUniTask(handler);
            scene = SceneManager.GetSceneAt(SceneManager.sceneCount - 1);
            UIManager.instance.Close(typeof(ILoading));
            SceneManager.sceneUnloaded += UnloadScene;
            return scene;
        }


        public GameObject Instantiate()
        {
            if (obj is null)
            {
                return default;
            }

            GameObject gameObject = (GameObject)GameObject.Instantiate(Get<GameObject>(default));
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
                    image.sprite = Get<Sprite>(gameObject);
                    break;
                case RawImage rawImage:
                    rawImage.texture = Get<Texture2D>(gameObject);
                    break;
                case AudioSource audioSource:
                    audioSource.clip = Get<AudioClip>(gameObject);
                    break;
                case VideoPlayer videoPlayer:
                    videoPlayer.clip = Get<VideoClip>(gameObject);
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
        }
    }
}