using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;
using ZGame.UI;
using Object = UnityEngine.Object;

namespace ZGame.Resource
{
    public sealed partial class ResObject : IDisposable
    {
        private object obj;
        private string _path;
        private int _refCount;
        private ResPackage parent;

        public string path
        {
            get { return _path; }
        }

        public int refCount
        {
            get { return _refCount; }
        }

        public Object Asset
        {
            get { return (Object)obj; }
        }

        public ResPackage Parent
        {
            get { return parent; }
        }

        internal static ResObject Create(ResPackage parent, object obj, string path)
        {
            ResObject resObject = new ResObject();
            resObject.obj = obj;
            resObject._path = path;
            resObject.parent = parent;
            resObject._refCount = 0;
            return resObject;
        }

        public static ResObject Create(Object obj, string path)
        {
            return Create(null, obj, path);
        }

        public bool IsSuccess()
        {
            if (obj != null)
            {
                return true;
            }

            return false;
        }

        public T GetAsset<T>()
        {
            if (obj == null)
            {
                return default;
            }

            object result = obj;
            if (obj is TextAsset textAsset)
            {
                if (typeof(T) == typeof(AudioClip))
                {
                    result = WavUtility.ToAudioClip(textAsset.bytes);
                }

                if (typeof(T) == typeof(Texture2D) || typeof(T) == typeof(Sprite))
                {
                    Texture2D texture2D = new Texture2D(0, 0, TextureFormat.RGBA32, false);
                    texture2D.LoadRawTextureData(textAsset.bytes);
                    texture2D.Apply();
                    if (typeof(T) == typeof(Sprite))
                    {
                        result = Sprite.Create(texture2D, new Rect(0, 0, texture2D.width, texture2D.height), Vector2.one / 2);
                    }
                    else
                    {
                        result = texture2D;
                    }
                }
            }

            parent?.Required();
            return (T)result;
        }

        public T GetAsset<T>(GameObject gameObject)
        {
            gameObject?.OnListenDestroyEvent(() => { Release(); });
            return GetAsset<T>();
        }

        public void Release(bool isClear = false)
        {
            _refCount--;
            parent?.Unrequire();
            if (isClear is false)
            {
                return;
            }

            for (int i = 0; i < _refCount; i++)
            {
                parent?.Unrequire();
            }

            obj = null;
            _refCount = 0;
            parent = null;
            _path = String.Empty;
        }

        public void Dispose()
        {
            Debug.Log("Dispose ResObject:" + path);
            Release(true);
            GC.SuppressFinalize(this);
        }
    }

    public sealed partial class ResObject
    {
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
            gameObject.OnListenDestroyEvent(() => { Release(); });
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