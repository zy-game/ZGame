using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using Object = UnityEngine.Object;

namespace ZEngine.Resource
{
    public interface IRequestAssetExecute<T> : IExecute where T : Object
    {
        T asset { get; }
        string path { get; }
        RuntimeBundleManifest manifest { get; }
        GameObject Instantiate();
        GameObject Instantiate(GameObject parent, Vector3 position, Vector3 rotation, Vector3 scale);
        AudioClip SetAudioClip(GameObject gameObject);
        Sprite SetSprite(GameObject gameObject);
        Texture2D SetTexture2D(GameObject gameObject);
        VideoClip SetVideoClip(GameObject gameObject);
        void Free();

        internal static IRequestAssetExecute<T> Create(string path)
        {
            InternalRequestAssetExecute<T> internalRequestAssetExecute = Activator.CreateInstance<InternalRequestAssetExecute<T>>();
            internalRequestAssetExecute.path = path;
            return internalRequestAssetExecute;
        }

        class InternalRequestAssetExecute<T> : IExecute, IRequestAssetExecute<T> where T : Object
        {
            public T asset { get; set; }
            public string path { get; set; }
            public RuntimeBundleManifest manifest { get; set; }
            private InternalRuntimeBundleHandle bundleHandle { get; set; }

            public void Execute()
            {
                if (path.IsNullOrEmpty())
                {
                    return;
                }

                if (path.StartsWith("Resources"))
                {
                    string temp = path.Substring("Resources/".Length);
                    asset = Resources.Load<T>(temp);
                    WaitFor.WaitFormFrameEnd(this.Dispose);
                    return;
                }
                else
                {
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

                    asset = bundleHandle.Load<T>(path);
                    WaitFor.WaitFormFrameEnd(this.Dispose);
                }
            }

            public void Dispose()
            {
                asset = null;
                GC.SuppressFinalize(this);
            }

            private void EnsureLoadSuccessfuly()
            {
                if (asset == null)
                {
                    throw new NullReferenceException("asset");
                }
            }

            public GameObject Instantiate()
            {
                return Instantiate(null, Vector3.zero, Vector3.zero, Vector3.zero);
            }

            public GameObject Instantiate(GameObject parent, Vector3 position, Vector3 rotation, Vector3 scale)
            {
                EnsureLoadSuccessfuly();
                GameObject gameObject = GameObject.Instantiate(asset) as GameObject;
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
                gameObject.OnDestroyEvent(Free);
                return gameObject;
            }

            public AudioClip SetAudioClip(GameObject gameObject)
            {
                AudioSource source = gameObject.GetComponent<AudioSource>();
                if (source == null)
                {
                    source = gameObject.AddComponent<AudioSource>();
                }

                source.clip = asset as AudioClip;
                gameObject.OnDestroyEvent(Free);
                return source.clip;
            }

            public Sprite SetSprite(GameObject gameObject)
            {
                Image source = gameObject.GetComponent<Image>();
                if (source == null)
                {
                    source = gameObject.AddComponent<Image>();
                }

                source.sprite = asset as Sprite;
                gameObject.OnDestroyEvent(Free);
                return source.sprite;
            }

            public Texture2D SetTexture2D(GameObject gameObject)
            {
                RawImage source = gameObject.GetComponent<RawImage>();
                if (source == null)
                {
                    source = gameObject.AddComponent<RawImage>();
                }

                source.texture = asset as Texture2D;
                gameObject.OnDestroyEvent(Free);
                return (Texture2D)source.texture;
            }

            public VideoClip SetVideoClip(GameObject gameObject)
            {
                VideoPlayer source = gameObject.GetComponent<VideoPlayer>();
                if (source == null)
                {
                    source = gameObject.AddComponent<VideoPlayer>();
                }

                gameObject.OnDestroyEvent(Free);
                source.clip = asset as VideoClip;
                return source.clip;
            }

            public void Free()
            {
                if (bundleHandle is null)
                {
                    return;
                }

                bundleHandle.Unload(asset);
            }
        }
    }
}