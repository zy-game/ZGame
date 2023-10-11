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
    public interface IRequestResourceModuleResult : IDisposable
    {
        Status status { get; }
        ModuleOptions[] options { get; }

        internal static UniTask<IRequestResourceModuleResult> Create(IGameProgressHandle gameProgressHandle, params ModuleOptions[] options)
        {
            UniTaskCompletionSource<IRequestResourceModuleResult> uniTaskCompletionSource = new UniTaskCompletionSource<IRequestResourceModuleResult>();
            InternalRequestResourceModuleResult internalRequestResourceModuleResult = Activator.CreateInstance<InternalRequestResourceModuleResult>();
            internalRequestResourceModuleResult.uniTaskCompletionSource = uniTaskCompletionSource;
            internalRequestResourceModuleResult.gameProgressHandle = gameProgressHandle;
            internalRequestResourceModuleResult.options = options;
            internalRequestResourceModuleResult.Execute();
            return uniTaskCompletionSource.Task;
        }

        class InternalRequestResourceModuleResult : IRequestResourceModuleResult
        {
            public Status status { get; set; }
            public ModuleOptions[] options { get; set; }
            public IGameProgressHandle gameProgressHandle;
            public UniTaskCompletionSource<IRequestResourceModuleResult> uniTaskCompletionSource;


            public void Dispose()
            {
                uniTaskCompletionSource = default;
                options = Array.Empty<ModuleOptions>();
                gameProgressHandle = null;
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
                    this.gameProgressHandle.SetProgress(1);
                    status = Status.Success;
                    uniTaskCompletionSource.TrySetResult(this);
                    return;
                }
#endif
                if (options is null || options.Length is 0)
                {
                    status = Status.Success;
                    this.gameProgressHandle.SetProgress(1);
                    uniTaskCompletionSource.TrySetResult(this);
                    return;
                }

                List<GameAssetBundleManifest> runtimeBundleManifests = new List<GameAssetBundleManifest>();
                for (int i = 0; i < options.Length; i++)
                {
                    GameResourceModuleManifest gameResourceModuleManifest = ResourceManager.instance.GetRuntimeModuleManifest(options[i].moduleName);
                    if (gameResourceModuleManifest is null)
                    {
                        Launche.Console.Log("获取资源模块信息失败，请确认在加载模块前已执行了模块更新检查", options[i].moduleName);
                        this.gameProgressHandle.SetProgress(1);
                        status = Status.Failed;
                        uniTaskCompletionSource.TrySetResult(this);
                        return;
                    }

                    if (gameResourceModuleManifest.bundleList is null || gameResourceModuleManifest.bundleList.Count is 0)
                    {
                        status = Status.Failed;
                        Launche.Console.Error("Not Find Bundle List:", options[i].moduleName);
                        this.gameProgressHandle.SetProgress(1);
                        uniTaskCompletionSource.TrySetResult(this);
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

                status = Status.Success;
                for (int i = 0; i < runtimeBundleManifests.Count; i++)
                {
                    IRequestAssetBundleResult requestAssetBundleExecuteHandles = await IRequestAssetBundleResult.Create(runtimeBundleManifests[i]);
                    if (requestAssetBundleExecuteHandles.status is not Status.Success)
                    {
                        status = Status.Failed;
                        this.gameProgressHandle.SetProgress(1);
                        uniTaskCompletionSource.TrySetResult(this);
                        return;
                    }

                    this.gameProgressHandle.SetProgress((float)i / (float)runtimeBundleManifests.Count);
                    requestAssetBundleExecuteHandles.Dispose();
                }

                this.gameProgressHandle.SetProgress(1);
                uniTaskCompletionSource.TrySetResult(this);
            }
        }
    }
}