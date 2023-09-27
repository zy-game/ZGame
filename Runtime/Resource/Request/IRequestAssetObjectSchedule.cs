using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using Object = UnityEngine.Object;

namespace ZEngine.Resource
{
    public interface IRequestAssetObjectSchedule<T> : ISchedule<T> where T : Object
    {
        string path { get; }
        GameAssetBundleManifest manifest { get; }

        void Release();

        public GameObject Instantiate()
        {
            return this.Instantiate(null, Vector3.zero, Vector3.zero, Vector3.zero);
        }

        public GameObject Instantiate(GameObject parent, Vector3 position, Vector3 rotation, Vector3 scale)
        {
            if (result == null)
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
            gameObject.OnDestroyEvent(() => { Engine.Resource.Release(temp); });
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
            gameObject.OnDestroyEvent(() => { Engine.Resource.Release(temp); });
        }

        internal static IRequestAssetObjectSchedule<T> Create(string path)
        {
            InternalRequestAssetObjectSchedule<T> internalRequestAssetObjectSchedule = Activator.CreateInstance<InternalRequestAssetObjectSchedule<T>>();
            internalRequestAssetObjectSchedule.path = path;
            return internalRequestAssetObjectSchedule;
        }

        class InternalRequestAssetObjectSchedule<T> : IRequestAssetObjectSchedule<T> where T : Object
        {
            public T result { get; set; }
            public string path { get; set; }
            public GameAssetBundleManifest manifest { get; set; }

            public void Release()
            {
                if (result == null)
                {
                    return;
                }

                Engine.Resource.Release(result);
                Dispose();
            }

            private AssetBundleRuntimeHandle bundleHandle { get; set; }

            public void Execute(params object[] args)
            {
                if (path.IsNullOrEmpty())
                {
                    return;
                }

                if (path.StartsWith("Resources"))
                {
                    string temp = path.Substring("Resources/".Length);
                    result = Resources.Load<T>(temp);
                    WaitFor.WaitFormFrameEnd(this.Dispose);
                    return;
                }
#if UNITY_EDITOR
                if (HotfixOptions.instance.useHotfix is Switch.Off || HotfixOptions.instance.useAsset == Switch.Off)
                {
                    result = UnityEditor.AssetDatabase.LoadAssetAtPath<T>(path);
                    return;
                }
#endif
                manifest = ResourceManager.instance.GetBundleManifestWithAssetPath(path);
                if (manifest is null)
                {
                    Engine.Console.Error("Not Find The Asset Bundle Manifest");
                    WaitFor.WaitFormFrameEnd(this.Dispose);
                    return;
                }

                bundleHandle = ResourceManager.instance.GetRuntimeAssetBundleHandle(manifest.owner, manifest.name);
                if (bundleHandle is null)
                {
                    Engine.Console.Error($"Not find the asset bundle:{manifest.name}.please check your is loaded the bundle");
                    WaitFor.WaitFormFrameEnd(this.Dispose);
                    return;
                }

                result = bundleHandle.Load<T>(path);
            }

            public void Dispose()
            {
                Engine.Console.Log("Dispose Asset Object Load Handle ->", result.name);
                result = null;
                path = String.Empty;
                manifest = null;
                bundleHandle = null;
                GC.SuppressFinalize(this);
            }
        }
    }
}