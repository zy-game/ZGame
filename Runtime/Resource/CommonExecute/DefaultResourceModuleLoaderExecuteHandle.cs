using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ZEngine.Resource
{
    /// <summary>
    /// 资源预加载
    /// </summary>
    public interface IResourceModuleLoaderExecuteHandle : IExecuteHandle<IResourceModuleLoaderExecuteHandle>
    {
        ModuleOptions[] options { get; }

        /// <summary>
        /// 订阅模块加载进度
        /// </summary>
        /// <param name="subscribe"></param>
        void SubscribeProgressChange(ISubscriber<float> subscribe);


        internal static IResourceModuleLoaderExecuteHandle Create(params ModuleOptions[] options)
        {
            InternalResourceModuleLoaderExecuteHandle internalResourceModuleLoaderExecuteHandle = Activator.CreateInstance<InternalResourceModuleLoaderExecuteHandle>();
            internalResourceModuleLoaderExecuteHandle.options = options;
            return internalResourceModuleLoaderExecuteHandle;
        }

        class InternalResourceModuleLoaderExecuteHandle : AbstractExecuteHandle, IExecuteHandle<IResourceModuleLoaderExecuteHandle>, IResourceModuleLoaderExecuteHandle
        {
            public ModuleOptions[] options { get; set; }
            private ISubscriber<float> subscribe;

            public void SubscribeProgressChange(ISubscriber<float> subscribe)
            {
                this.subscribe = subscribe;
            }

            public override void Dispose()
            {
                base.Dispose();
                options = Array.Empty<ModuleOptions>();
                subscribe?.Dispose();
                subscribe = null;
            }

            protected override IEnumerator OnExecute()
            {
#if UNITY_EDITOR
                if (HotfixOptions.instance.useHotfix is Switch.Off || HotfixOptions.instance.useAsset is Switch.Off)
                {
                    status = Status.Success;
                    yield break;
                }
#endif
                if (options is null || options.Length is 0)
                {
                    status = Status.Success;
                    this.subscribe.Execute(1);
                    yield break;
                }

                List<RuntimeBundleManifest> runtimeBundleManifests = new List<RuntimeBundleManifest>();
                for (int i = 0; i < options.Length; i++)
                {
                    RuntimeModuleManifest runtimeModuleManifest = ResourceManager.instance.GetRuntimeModuleManifest(options[i].moduleName);
                    if (runtimeModuleManifest is null)
                    {
                        Engine.Console.Log("获取资源模块信息失败，请确认在加载模块前已执行了模块更新检查", options[i].moduleName);
                        break;
                    }

                    if (runtimeModuleManifest.bundleList is null || runtimeModuleManifest.bundleList.Count is 0)
                    {
                        status = Status.Failed;
                        Engine.Console.Error("Not Find Bundle List:", options[i].moduleName);
                        continue;
                    }

                    foreach (var UPPER in runtimeModuleManifest.bundleList)
                    {
                        if (ResourceManager.instance.HasLoadAssetBundle(runtimeModuleManifest.name, UPPER.name))
                        {
                            continue;
                        }

                        runtimeBundleManifests.Add(UPPER);
                    }
                }

                IRequestAssetBundleExecuteHandle[] requestAssetBundleExecuteHandles = new IRequestAssetBundleExecuteHandle[runtimeBundleManifests.Count];
                //todo 开始加载资源包
                for (int i = 0; i < runtimeBundleManifests.Count; i++)
                {
                    requestAssetBundleExecuteHandles[i] = IRequestAssetBundleExecuteHandle.Create(runtimeBundleManifests[i]);
                    requestAssetBundleExecuteHandles[i].Execute();
                }

                yield return WaitFor.Create(() =>
                {
                    int count = requestAssetBundleExecuteHandles.Where(x => x.status == Status.Failed || x.status == Status.Success).Count();
                    this.subscribe?.Execute((float)count / (float)requestAssetBundleExecuteHandles.Length);
                    return count == requestAssetBundleExecuteHandles.Length;
                });

                status = Status.Success;
            }
        }
    }
}