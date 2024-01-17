using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Cysharp.Threading.Tasks;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;
using ZGame.Window;
using Object = UnityEngine.Object;

namespace ZGame.Resource
{
    public sealed class ResObject : IDisposable
    {
        private object obj;
        private PackageHandle parent;
        public string path { get; private set; }
        public int refCount { get; private set; }

        public string packageName
        {
            get { return parent.name; }
        }

        public static ResObject OnCreate([NotNull] PackageHandle parent, [NotNull] object obj, [NotNull] string path)
        {
            ResObject resObject = new ResObject()
            {
                obj = obj,
                path = path,
                parent = parent
            };
            ResObjectCache.instance.Add(resObject);
            return resObject;
        }


        public bool IsSuccess()
        {
            if (obj != null)
            {
                return true;
            }

            return false;
        }

        // public T Get<T>(GameObject gameObject = null)
        // {
        //     gameObject?.OnListenDestroyEvent(Release);
        //     parent.AddRef();
        //     return obj == null ? default(T) : (T)obj;
        // }

        public T GetAsset<T>()
        {
            if (obj == null)
            {
                return default;
            }

            parent.AddRef();
            return (T)obj;
        }

        public T GetAsset<T>(GameObject gameObject)
        {
            gameObject?.OnListenDestroyEvent(Release);
            return GetAsset<T>();
        }

        public void Release()
        {
            refCount--;
            parent.MinusRef();
        }

        public void Dispose(bool isClear)
        {
            if (refCount > 0)
            {
                return;
            }

            refCount = 0;
            obj = null;
            parent = null;
            path = String.Empty;
        }

        public void Dispose()
        {
            Release();
        }

        public Scene OpenScene()
        {
            Scene scene = GetAsset<Scene>();

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
            Scene scene = GetAsset<Scene>();

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

            GameObject templete = GetAsset<GameObject>();
            GameObject gameObject = (GameObject)GameObject.Instantiate(templete);
            gameObject.OnListenDestroyEvent(Release);
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

        public void SetSprite(Image image)
        {
            if (image == null)
            {
                return;
            }

            image.sprite = GetAsset<Sprite>(image.gameObject);
        }

        public void SetTexture2D(RawImage image)
        {
            if (image == null)
            {
                return;
            }

            image.texture = GetAsset<Texture2D>(image.gameObject);
        }

        public void SetMaterialTexture2D(Material material, string propertyName, GameObject gameObject)
        {
            if (material == null)
            {
                return;
            }

            material.SetTexture(propertyName, GetAsset<Texture2D>(gameObject));
        }

        public void SetRenderMaterial(Renderer renderer)
        {
            if (renderer == null)
            {
                return;
            }

            renderer.sharedMaterial = GetAsset<Material>(renderer.gameObject);
        }

        public void SetGraphicMaterial(MaskableGraphic graphic)
        {
            if (graphic == null)
            {
                return;
            }

            graphic.material = GetAsset<Material>(graphic.gameObject);
        }
    }
}