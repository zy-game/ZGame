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
    public interface IRequestAssetObjectScheduleHandle<T> : IRequestAssetObjectSchedule<T>, IScheduleHandle<T> where T : Object
    {
        string path { get; }
        GameAssetBundleManifest manifest { get; }

        internal static IRequestAssetObjectScheduleHandle<T> Create(string path)
        {
            InternalRequestAssetObjectScheduleHandle<T> internalRequestAssetObjectScheduleHandle = Activator.CreateInstance<InternalRequestAssetObjectScheduleHandle<T>>();
            internalRequestAssetObjectScheduleHandle.path = path;
            return internalRequestAssetObjectScheduleHandle;
        }

        class InternalRequestAssetObjectScheduleHandle<T> : IRequestAssetObjectScheduleHandle<T> where T : Object
        {
            public T result { get; set; }
            public string path { get; set; }
            public Status status { get; set; }
            public GameAssetBundleManifest manifest { get; set; }
            private AssetBundleRuntimeHandle bundleHandle { get; set; }
            private ISubscriber subscriber;

            public void Dispose()
            {
                Engine.Console.Log("Dispose Asset Object Load Handle ->", result.name);
                result = null;
                path = String.Empty;
                status = Status.None;
                manifest = null;
                bundleHandle = null;
                subscriber?.Dispose();
                subscriber = null;
            }

            public void Release()
            {
                if (result == null)
                {
                    return;
                }

                Engine.Resource.Release(result);
                Dispose();
            }

            public void Execute(params object[] args)
            {
                if (status is Status.None)
                {
                    status = Status.Execute;
                    DOExecute().StartCoroutine(OnComplate);
                }
            }

            private void OnComplate()
            {
                if (subscriber is not null)
                {
                    subscriber.Execute(this);
                }
            }

            public void Subscribe(ISubscriber subscriber)
            {
                if (this.subscriber is null)
                {
                    this.subscriber = subscriber;
                    return;
                }

                this.subscriber.Merge(subscriber);
            }

            protected IEnumerator DOExecute()
            {
                if (path.IsNullOrEmpty())
                {
                    Engine.Console.Error("The Asset Path Is Null Or Empty");
                    status = Status.Failed;
                    yield break;
                }

                if (path.StartsWith("Resources"))
                {
                    string temp = path.Substring("Resources/".Length);
                    result = Resources.Load<T>(temp);
                    status = Status.Success;
                    yield break;
                }
#if UNITY_EDITOR
                if (HotfixOptions.instance.useHotfix is Switch.Off || HotfixOptions.instance.useAsset == Switch.Off)
                {
                    result = UnityEditor.AssetDatabase.LoadAssetAtPath<T>(path);
                    status = Status.Success;
                    yield break;
                }
#endif
                GameAssetBundleManifest manifest = ResourceManager.instance.GetBundleManifestWithAssetPath(path);
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

                yield return bundleHandle.LoadAsync<T>(path, ISubscriber.Create<T>(args => result = args));
                status = Status.Success;
            }
        }
    }
}