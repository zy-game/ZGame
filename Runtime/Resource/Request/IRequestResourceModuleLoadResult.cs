using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;

namespace ZEngine.Resource
{
    /// <summary>
    /// 资源模块加载结果
    /// </summary>
    public interface IRequestResourceModuleLoadResult : IDisposable
    {
        Status status { get; }
        ModuleOptions[] options { get; }

        internal static UniTask<IRequestResourceModuleLoadResult> Create(IProgressHandle gameProgressHandle, params ModuleOptions[] options)
        {
            UniTaskCompletionSource<IRequestResourceModuleLoadResult> uniTaskCompletionSource = new UniTaskCompletionSource<IRequestResourceModuleLoadResult>();
            InternalRequestResourceModuleResult internalRequestResourceModuleResult = Activator.CreateInstance<InternalRequestResourceModuleResult>();
            internalRequestResourceModuleResult.uniTaskCompletionSource = uniTaskCompletionSource;
            internalRequestResourceModuleResult.gameProgressHandle = gameProgressHandle;
            internalRequestResourceModuleResult.options = options;
            internalRequestResourceModuleResult.Execute();
            return uniTaskCompletionSource.Task;
        }

        class InternalRequestResourceModuleResult : IRequestResourceModuleLoadResult
        {
            public Status status { get; set; }
            public ModuleOptions[] options { get; set; }
            public IProgressHandle gameProgressHandle;
            public UniTaskCompletionSource<IRequestResourceModuleLoadResult> uniTaskCompletionSource;


            public void Dispose()
            {
                uniTaskCompletionSource = default;
                options = Array.Empty<ModuleOptions>();
                gameProgressHandle = null;
            }

            private void OnComplate(Status status)
            {
                this.gameProgressHandle.SetProgress(1);
                this.status = status;
                uniTaskCompletionSource.TrySetResult(this);
            }

            public async void Execute()
            {
                if (status is not Status.None)
                {
                    return;
                }

#if UNITY_EDITOR
                if (HotfixOptions.instance.useHotfix is Switch.Off || HotfixOptions.instance.useAsset is Switch.Off)
                {
                    OnComplate(Status.Success);
                    return;
                }
#endif
                if (options is null || options.Length is 0)
                {
                    OnComplate(Status.Success);
                    return;
                }

                List<ResourceBundleManifest> runtimeBundleManifests = new List<ResourceBundleManifest>();
                for (int i = 0; i < options.Length; i++)
                {
                    ResourceModuleManifest resourceModuleManifest = ZGame.Data.GetRuntimeDatableHandle<ResourceModuleManifest>(options[i].moduleName); 
                    if (resourceModuleManifest is null || resourceModuleManifest.bundleList is null || resourceModuleManifest.bundleList.Count is 0)
                    {
                        ZGame.Console.Log("获取资源模块信息失败，请确认在加载模块前已执行了模块更新检查", options[i].moduleName);
                        OnComplate(Status.Failed);
                        return;
                    }

                    foreach (var UPPER in resourceModuleManifest.bundleList)
                    {
                        if (ZGame.Data.Equals<RuntimeAssetBundleHandle>(UPPER.name))
                        {
                            continue;
                        }

                        runtimeBundleManifests.Add(UPPER);
                    }
                }

                for (int i = 0; i < runtimeBundleManifests.Count; i++)
                {
                    IRequestResourceBundleResult requestAssetBundleExecuteHandles = await IRequestResourceBundleResult.Create(runtimeBundleManifests[i]);
                    if (requestAssetBundleExecuteHandles.status is not Status.Success)
                    {
                        OnComplate(Status.Failed);
                        return;
                    }

                    this.gameProgressHandle.SetProgress((float)i / (float)runtimeBundleManifests.Count);
                    requestAssetBundleExecuteHandles.Dispose();
                }

                OnComplate(Status.Success);
            }
        }
    }
}