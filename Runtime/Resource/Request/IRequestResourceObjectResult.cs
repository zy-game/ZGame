using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using Object = UnityEngine.Object;

namespace ZEngine.Resource
{
    public interface IRequestResourceObjectResult : IDisposable
    {
        Object result { get; }
        string path { get; }
        Status status { get; }

        void Release();


        internal static IRequestResourceObjectResult Create(string path)
        {
            InternalRequestResourceObjectResult internalRequestResourceObjectResult = Activator.CreateInstance<InternalRequestResourceObjectResult>();
            internalRequestResourceObjectResult.path = path;
            internalRequestResourceObjectResult.Execute();
            return internalRequestResourceObjectResult;
        }

        internal static UniTask<IRequestResourceObjectResult> CreateAsync(string path)
        {
            UniTaskCompletionSource<IRequestResourceObjectResult> uniTaskCompletionSource = new UniTaskCompletionSource<IRequestResourceObjectResult>();
            InternalRequestResourceObjectResult internalRequestResourceObjectResult = Activator.CreateInstance<InternalRequestResourceObjectResult>();
            internalRequestResourceObjectResult.path = path;
            internalRequestResourceObjectResult.ExecuteAsync(uniTaskCompletionSource);
            return uniTaskCompletionSource.Task;
        }

        class InternalRequestResourceObjectResult : IRequestResourceObjectResult
        {
            public Object result { get; set; }
            public string path { get; set; }
            public Status status { get; set; }

            private RuntimeAssetBundleHandle bundleHandle { get; set; }

            public async void ExecuteAsync(UniTaskCompletionSource<IRequestResourceObjectResult> uniTaskCompletionSource)
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
                bundleHandle = ZGame.Data.Find<RuntimeAssetBundleHandle>(x => x.Contains(path));
                if (bundleHandle is null)
                {
                    ZGame.Console.Error($"未找到资源,请确认资源包是否已经加载", path);
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

                bundleHandle =ZGame.Data.Find<RuntimeAssetBundleHandle>(x => x.Contains(path));
                if (bundleHandle is null)
                {
                    ZGame.Console.Error($"未找到资源,请确认资源包是否已经加载", path);
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