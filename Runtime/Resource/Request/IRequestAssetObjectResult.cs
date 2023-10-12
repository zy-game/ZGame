using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using Object = UnityEngine.Object;

namespace ZEngine.Resource
{
    public interface IRequestAssetObjectResult : IDisposable
    {
        Object result { get; }
        string path { get; }
        Status status { get; }

        void Release();


        internal static IRequestAssetObjectResult Create(string path)
        {
            InternalRequestAssetObjectResult internalRequestAssetObjectResult = Activator.CreateInstance<InternalRequestAssetObjectResult>();
            internalRequestAssetObjectResult.path = path;
            internalRequestAssetObjectResult.Execute();
            return internalRequestAssetObjectResult;
        }

        internal static UniTask<IRequestAssetObjectResult> CreateAsync(string path)
        {
            UniTaskCompletionSource<IRequestAssetObjectResult> uniTaskCompletionSource = new UniTaskCompletionSource<IRequestAssetObjectResult>();
            InternalRequestAssetObjectResult internalRequestAssetObjectResult = Activator.CreateInstance<InternalRequestAssetObjectResult>();
            internalRequestAssetObjectResult.path = path;
            internalRequestAssetObjectResult.ExecuteAsync(uniTaskCompletionSource);
            return uniTaskCompletionSource.Task;
        }

        class InternalRequestAssetObjectResult : IRequestAssetObjectResult
        {
            public Object result { get; set; }
            public string path { get; set; }
            public Status status { get; set; }

            private AssetBundleRuntimeHandle bundleHandle { get; set; }

            public async void ExecuteAsync(UniTaskCompletionSource<IRequestAssetObjectResult> uniTaskCompletionSource)
            {
                if (path.IsNullOrEmpty())
                {
                    ZGame.Console.Error("The Asset Path Is Null Or Empty");
                    status = Status.Failed;
                    uniTaskCompletionSource.TrySetResult(this);
                    return;
                }

                if (path.StartsWith("Resources"))
                {
                    string temp = path.Substring("Resources/".Length);
                    result = await Resources.LoadAsync(temp);
                    status = Status.Success;
                    uniTaskCompletionSource.TrySetResult(this);
                    return;
                }
#if UNITY_EDITOR
                if (HotfixOptions.instance.useHotfix is Switch.Off || HotfixOptions.instance.useAsset == Switch.Off)
                {
                    result = UnityEditor.AssetDatabase.LoadAssetAtPath(path, typeof(Object));
                    status = Status.Success;
                    uniTaskCompletionSource.TrySetResult(this);
                    return;
                }
#endif
                GameAssetBundleManifest manifest = ResourceManager.instance.GetBundleManifestWithAssetPath(path);
                if (manifest is null)
                {
                    ZGame.Console.Error("Not Find The Asset Bundle Manifest");
                    status = Status.Failed;
                    uniTaskCompletionSource.TrySetResult(this);
                    return;
                }

                bundleHandle = ResourceManager.instance.GetRuntimeAssetBundleHandle(manifest.owner, manifest.name);
                if (bundleHandle is null)
                {
                    ZGame.Console.Error($"Not find the asset bundle:{manifest.name}.please check your is loaded the bundle");
                    status = Status.Failed;
                    uniTaskCompletionSource.TrySetResult(this);
                    return;
                }

                result = await bundleHandle.LoadAsync(path);
                status = Status.Success;
                uniTaskCompletionSource.TrySetResult(this);
            }

            public void Execute()
            {
                if (path.IsNullOrEmpty())
                {
                    status = Status.Failed;
                    ZGame.Console.Error("资源路径不能为空");
                    return;
                }

                if (path.StartsWith("Resources"))
                {
                    string temp = path.Substring("Resources/".Length);
                    result = Resources.Load(temp);
                    status = result == null ? Status.Failed : Status.Success;
                    return;
                }
#if UNITY_EDITOR
                if (HotfixOptions.instance.useHotfix is Switch.Off || HotfixOptions.instance.useAsset == Switch.Off)
                {
                    result = UnityEditor.AssetDatabase.LoadAssetAtPath(path, typeof(Object));
                    status = result == null ? Status.Failed : Status.Success;
                    return;
                }
#endif
                GameAssetBundleManifest manifest = ResourceManager.instance.GetBundleManifestWithAssetPath(path);
                if (manifest is null)
                {
                    ZGame.Console.Error("Not Find The Asset Bundle Manifest");
                    status = Status.Failed;
                    return;
                }

                bundleHandle = ResourceManager.instance.GetRuntimeAssetBundleHandle(manifest.owner, manifest.name);
                if (bundleHandle is null)
                {
                    ZGame.Console.Error($"Not find the asset bundle:{manifest.name}.please check your is loaded the bundle");
                    status = Status.Failed;
                    return;
                }

                result = bundleHandle.Load(path);
                status = result == null ? Status.Failed : Status.Success;
            }

            public void Release()
            {
                if (result == null)
                {
                    return;
                }

                ZGame.Resource.Release(result);
                Dispose();
            }

            public void Dispose()
            {
                ZGame.Console.Log("Dispose Asset Object Load Handle ->", result.name);
                result = null;
                path = String.Empty;
                bundleHandle = null;
                GC.SuppressFinalize(this);
            }
        }
    }
}