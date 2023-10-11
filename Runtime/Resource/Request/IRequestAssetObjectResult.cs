using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using Object = UnityEngine.Object;

namespace ZEngine.Resource
{
    public interface IRequestAssetObjectResult<T> : IDisposable where T : Object
    {
        T result { get; }
        string path { get; }
        Status status { get; }
        GameAssetBundleManifest manifest { get; }

        void Release();

        public GameObject Instantiate()
        {
            return this.Instantiate(null, Vector3.zero, Vector3.zero, Vector3.zero);
        }

        public GameObject Instantiate(GameObject parent, Vector3 position, Vector3 rotation, Vector3 scale)
        {
            if (result == null || typeof(GameObject) != typeof(T))
            {
                return default;
            }

            GameObject gameObject = GameObject.Instantiate(result) as GameObject;
            if (gameObject == null)
            {
                return default;
            }

            if (parent != null)
            {
                gameObject.transform.SetParent(parent.transform);
            }

            gameObject.transform.position = position;
            gameObject.transform.rotation = Quaternion.Euler(rotation);
            gameObject.transform.localScale = scale;
            Object temp = result;
            gameObject.OnDestroyEvent(() => { Launche.Resource.Release(temp); });
            return gameObject;
        }

        public void SetAssetObject<T>(GameObject gameObject) where T : Component
        {
            if (result == null)
            {
                return;
            }

            T component = gameObject.GetComponent<T>();
            switch (component)
            {
                case AudioSource audioSource:
                    audioSource.clip = result as AudioClip;
                    break;
                case Image spriteImage:
                    spriteImage.sprite = result as Sprite;
                    break;
                case RawImage rawImage:
                    rawImage.texture = result as Texture2D;
                    break;
                case VideoPlayer videoPlayer:
                    videoPlayer.clip = result as VideoClip;
                    break;
            }

            Object temp = result;
            gameObject.OnDestroyEvent(() => { Launche.Resource.Release(temp); });
        }

        internal static IRequestAssetObjectResult<T> Create(string path)
        {
            InternalRequestAssetObjectResult<T> internalRequestAssetObjectResult = Activator.CreateInstance<InternalRequestAssetObjectResult<T>>();
            internalRequestAssetObjectResult.path = path;
            internalRequestAssetObjectResult.Execute();
            return internalRequestAssetObjectResult;
        }

        internal static UniTask<IRequestAssetObjectResult<T>> CreateAsync(string path)
        {
            UniTaskCompletionSource<IRequestAssetObjectResult<T>> uniTaskCompletionSource = new UniTaskCompletionSource<IRequestAssetObjectResult<T>>();
            InternalRequestAssetObjectResult<T> internalRequestAssetObjectResult = Activator.CreateInstance<InternalRequestAssetObjectResult<T>>();
            internalRequestAssetObjectResult.path = path;
            internalRequestAssetObjectResult.ExecuteAsync(uniTaskCompletionSource);
            return uniTaskCompletionSource.Task;
        }

        class InternalRequestAssetObjectResult<T> : IRequestAssetObjectResult<T> where T : Object
        {
            public T result { get; set; }
            public string path { get; set; }
            public Status status { get; set; }
            public GameAssetBundleManifest manifest { get; set; }

            private AssetBundleRuntimeHandle bundleHandle { get; set; }

            public async void ExecuteAsync(UniTaskCompletionSource<IRequestAssetObjectResult<T>> uniTaskCompletionSource)
            {
                if (path.IsNullOrEmpty())
                {
                    Launche.Console.Error("The Asset Path Is Null Or Empty");
                    status = Status.Failed;
                    uniTaskCompletionSource.TrySetResult(this);
                    return;
                }

                if (path.StartsWith("Resources"))
                {
                    string temp = path.Substring("Resources/".Length);
                    result = (T)await Resources.LoadAsync<T>(temp);
                    status = Status.Success;
                    uniTaskCompletionSource.TrySetResult(this);
                    return;
                }
#if UNITY_EDITOR
                if (HotfixOptions.instance.useHotfix is Switch.Off || HotfixOptions.instance.useAsset == Switch.Off)
                {
                    result = UnityEditor.AssetDatabase.LoadAssetAtPath<T>(path);
                    status = Status.Success;
                    uniTaskCompletionSource.TrySetResult(this);
                    return;
                }
#endif
                GameAssetBundleManifest manifest = ResourceManager.instance.GetBundleManifestWithAssetPath(path);
                if (manifest is null)
                {
                    Launche.Console.Error("Not Find The Asset Bundle Manifest");
                    status = Status.Failed;
                    uniTaskCompletionSource.TrySetResult(this);
                    return;
                }

                bundleHandle = ResourceManager.instance.GetRuntimeAssetBundleHandle(manifest.owner, manifest.name);
                if (bundleHandle is null)
                {
                    Launche.Console.Error($"Not find the asset bundle:{manifest.name}.please check your is loaded the bundle");
                    status = Status.Failed;
                    uniTaskCompletionSource.TrySetResult(this);
                    return;
                }

                result = await bundleHandle.LoadAsync<T>(path);
                status = Status.Success;
                uniTaskCompletionSource.TrySetResult(this);
            }

            public void Execute()
            {
                if (path.IsNullOrEmpty())
                {
                    status = Status.Failed;
                    Launche.Console.Error("资源路径不能为空");
                    return;
                }

                if (path.StartsWith("Resources"))
                {
                    string temp = path.Substring("Resources/".Length);
                    result = Resources.Load<T>(temp);
                    status = result == null ? Status.Failed : Status.Success;
                    return;
                }
#if UNITY_EDITOR
                if (HotfixOptions.instance.useHotfix is Switch.Off || HotfixOptions.instance.useAsset == Switch.Off)
                {
                    result = UnityEditor.AssetDatabase.LoadAssetAtPath<T>(path);
                    status = result == null ? Status.Failed : Status.Success;
                    return;
                }
#endif
                manifest = ResourceManager.instance.GetBundleManifestWithAssetPath(path);
                if (manifest is null)
                {
                    Launche.Console.Error("Not Find The Asset Bundle Manifest");
                    status = Status.Failed;
                    return;
                }

                bundleHandle = ResourceManager.instance.GetRuntimeAssetBundleHandle(manifest.owner, manifest.name);
                if (bundleHandle is null)
                {
                    Launche.Console.Error($"Not find the asset bundle:{manifest.name}.please check your is loaded the bundle");
                    status = Status.Failed;
                    return;
                }

                result = bundleHandle.Load<T>(path);
                status = result == null ? Status.Failed : Status.Success;
            }

            public void Release()
            {
                if (result == null)
                {
                    return;
                }

                Launche.Resource.Release(result);
                Dispose();
            }

            public void Dispose()
            {
                Launche.Console.Log("Dispose Asset Object Load Handle ->", result.name);
                result = null;
                path = String.Empty;
                manifest = null;
                bundleHandle = null;
                GC.SuppressFinalize(this);
            }
        }
    }
}