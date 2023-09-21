using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using Object = UnityEngine.Object;

namespace ZEngine.Resource
{
    /// <summary>
    /// 资源加载
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IRequestAssetExecuteHandle<T> : IRequestAssetExecute<T>, IExecuteHandle<IRequestAssetExecuteHandle<T>> where T : Object
    {
        internal static IRequestAssetExecuteHandle<T> Create(string path)
        {
            InternalRequestAssetExecuteHandle<T> internalRequestAssetExecuteHandle = Activator.CreateInstance<InternalRequestAssetExecuteHandle<T>>();
            internalRequestAssetExecuteHandle.path = path;
            return internalRequestAssetExecuteHandle;
        }

        class InternalRequestAssetExecuteHandle<T> : AbstractExecuteHandle, IRequestAssetExecuteHandle<T> where T : Object
        {
            public T asset { get; set; }
            public string path { get; set; }
            public RuntimeBundleManifest manifest { get; set; }
            private InternalRuntimeBundleHandle bundleHandle { get; set; }

            protected override IEnumerator ExecuteCoroutine()
            {
                if (path.IsNullOrEmpty())
                {
                    Engine.Console.Error("The Asset Path Is Null Or Empty");
                    status = Status.Failed;
                    yield break;
                }

                RuntimeBundleManifest manifest = ResourceManager.instance.GetBundleManifestWithAssetPath(path);
                if (manifest is null)
                {
                    Engine.Console.Error("Not Find The Asset Bundle Manifest");
                    status = Status.Failed;
                    yield break;
                }

                bundleHandle = ResourceManager.instance.GetRuntimeAssetBundleHandle(manifest.owner, manifest.name);
                if (bundleHandle is null)
                {
                    Engine.Console.Error($"Not find the asset bundle:{manifest.name}.please check your is loaded the bundle");
                    status = Status.Failed;
                    yield break;
                }

                yield return bundleHandle.LoadAsync<T>(path, ISubscribeHandle<T>.Create(args => asset = args));
                status = Status.Success;
            }

            private void EnsureLoadSuccessfuly()
            {
                if (asset == null)
                {
                    throw EngineException.Create<NullReferenceException>("asset");
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
                UnityEventArgs.Subscribe(GameEventType.OnDestroy, ISubscribeHandle<UnityEventArgs>.Create(args => bundleHandle.Unload(asset)), gameObject);
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
                UnityEventArgs.Subscribe(GameEventType.OnDestroy, ISubscribeHandle<UnityEventArgs>.Create(args => bundleHandle.Unload(asset)), gameObject);
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
                UnityEventArgs.Subscribe(GameEventType.OnDestroy, ISubscribeHandle<UnityEventArgs>.Create(args => bundleHandle.Unload(asset)), gameObject);
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
                UnityEventArgs.Subscribe(GameEventType.OnDestroy, ISubscribeHandle<UnityEventArgs>.Create(args => bundleHandle.Unload(asset)), gameObject);
                return (Texture2D)source.texture;
            }

            public VideoClip SetVideoClip(GameObject gameObject)
            {
                VideoPlayer source = gameObject.GetComponent<VideoPlayer>();
                if (source == null)
                {
                    source = gameObject.AddComponent<VideoPlayer>();
                }

                UnityEventArgs.Subscribe(GameEventType.OnDestroy, ISubscribeHandle<UnityEventArgs>.Create(args => bundleHandle.Unload(asset)), gameObject);
                source.clip = asset as VideoClip;
                return source.clip;
            }

            public void Free()
            {
                bundleHandle.Unload(asset);
            }
        }
    }
}