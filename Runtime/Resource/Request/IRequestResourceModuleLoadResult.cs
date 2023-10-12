using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;

namespace ZEngine.Resource
{
    /// <summary>
    /// 资源预加载
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

                List<GameAssetBundleManifest> runtimeBundleManifests = new List<GameAssetBundleManifest>();
                for (int i = 0; i < options.Length; i++)
                {
                    GameResourceModuleManifest gameResourceModuleManifest = ResourceManager.instance.GetRuntimeModuleManifest(options[i].moduleName);
                    if (gameResourceModuleManifest is null || gameResourceModuleManifest.bundleList is null || gameResourceModuleManifest.bundleList.Count is 0)
                    {
                        ZGame.Console.Log("获取资源模块信息失败，请确认在加载模块前已执行了模块更新检查", options[i].moduleName);
                        OnComplate(Status.Failed);
                        return;
                    }

                    foreach (var UPPER in gameResourceModuleManifest.bundleList)
                    {
                        if (ResourceManager.instance.HasLoadAssetBundle(gameResourceModuleManifest.name, UPPER.name))
                        {
                            continue;
                        }

                        runtimeBundleManifests.Add(UPPER);
                    }
                }

                for (int i = 0; i < runtimeBundleManifests.Count; i++)
                {
                    IRequestAssetBundleResult requestAssetBundleExecuteHandles = await IRequestAssetBundleResult.Create(runtimeBundleManifests[i]);
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