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
        private object obj;
        private PackageHandle parent;
        public string path { get; private set; }
        public int refCount { get; private set; }

        public string packageName
        {
            get { return parent.name; }
        }

        public static ResHandle OnCreate(PackageHandle parent, object obj, string path)
        {
            ResHandle handle = new ResHandle()
            {
                obj = obj,
                path = path,
                parent = parent
            };
            ResHandleCache.instance.Add(handle);
            return handle;
        }


        public bool Is<T>()
        {
            return obj is T;
        }

        public bool IsSuccess()
        {
            if (obj != null)
            {
                return true;
            }

            return false;
        }

        public T Get<T>(GameObject gameObject = null)
        {
            ListenerDestroyEvent(gameObject);
            parent?.Reference();
            return obj == null ? default(T) : (T)obj;
        }

        private void ListenerDestroyEvent(GameObject gameObject)
        {
            if (gameObject == null)
            {
                return;
            }

            BevaviourScriptable bevaviour = gameObject.GetComponent<BevaviourScriptable>();
            if (bevaviour == null)
            {
                bevaviour = gameObject.AddComponent<BevaviourScriptable>();
            }

            bevaviour.onDestroy.AddListener(() => { this.Release(); });
        }

        public void Release()
        {
            parent?.Unreference();
        }

        public void Dispose()
        {
            refCount = 0;
            obj = null;
            parent = null;
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
            if (BasicConfig.instance.resMode == ResourceMode.Editor)
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
            if (BasicConfig.instance.resMode == ResourceMode.Editor)
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
                    ListenerDestroyEvent(gameObject);
                    break;
                case RawImage rawImage:
                    rawImage.texture = Get<Texture2D>(gameObject);
                    ListenerDestroyEvent(gameObject);
                    break;
                case AudioSource audioSource:
                    audioSource.clip = Get<AudioClip>(gameObject);
                    ListenerDestroyEvent(gameObject);
                    break;
                case VideoPlayer videoPlayer:
                    videoPlayer.clip = Get<VideoClip>(gameObject);
                    ListenerDestroyEvent(gameObject);
                    break;
                case TMP_InputField inputField:
                    switch (obj)
                    {
                        case TextAsset textAsset:
                            inputField.text = textAsset.text;
                            ListenerDestroyEvent(gameObject);
                            break;
                        case TMP_FontAsset fontAsset:
                            inputField.fontAsset = fontAsset;
                            ListenerDestroyEvent(gameObject);
                            break;
                    }

                    break;
                case TMP_Text tmpText:
                    switch (obj)
                    {
                        case TextAsset textAsset:
                            tmpText.text = textAsset.text;
                            ListenerDestroyEvent(gameObject);
                            break;
                        case TMP_FontAsset fontAsset:
                            tmpText.font = fontAsset;
                            ListenerDestroyEvent(gameObject);
                            break;
                    }

                    break;
            }
        }
    }
}