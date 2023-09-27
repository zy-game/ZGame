using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ZEngine.Resource
{
    /// <summary>
    /// 资源预加载
    /// </summary>
    public interface IResourceModuleLoaderScheduleHandle : IScheduleHandle<IResourceModuleLoaderScheduleHandle>
    {
        ModuleOptions[] options { get; }

        /// <summary>
        /// 订阅模块加载进度
        /// </summary>
        /// <param name="subscribe"></param>
        void SubscribeProgressChange(ISubscriber<float> subscribe);


        internal static IResourceModuleLoaderScheduleHandle Create(params ModuleOptions[] options)
        {
            InternalResourceModuleLoaderScheduleHandle internalResourceModuleLoaderScheduleHandle = Activator.CreateInstance<InternalResourceModuleLoaderScheduleHandle>();
            internalResourceModuleLoaderScheduleHandle.options = options;
            return internalResourceModuleLoaderScheduleHandle;
        }

        class InternalResourceModuleLoaderScheduleHandle : IResourceModuleLoaderScheduleHandle
        {
            public Status status { get; set; }
            public ModuleOptions[] options { get; set; }
            private ISubscriber<float> subscribe;
            private ISubscriber complate;

            public void Subscribe(ISubscriber subscriber)
            {
                if (this.complate is null)
                {
                    this.complate = subscriber;
                    return;
                }

                this.complate.Merge(subscriber);
            }

            public IResourceModuleLoaderScheduleHandle result { get; }

            public void SubscribeProgressChange(ISubscriber<float> subscribe)
            {
                this.subscribe = subscribe;
            }

            public void Dispose()
            {
                options = Array.Empty<ModuleOptions>();
                subscribe?.Dispose();
                subscribe = null;
            }

            public void Execute(params object[] args)
            {
                if (status is not Status.None)
                {
                    return;
                }

                status = Status.Execute;
                DOExecute().StartCoroutine(OnComplate);
            }

            private void OnComplate()
            {
                if (complate is not null)
                {
                    complate.Execute(this);
                }
            }

            private IEnumerator DOExecute()
            {
#if UNITY_EDITOR
                if (HotfixOptions.instance.useHotfix is Switch.Off || HotfixOptions.instance.useAsset is Switch.Off)
                {
                    this.subscribe.Execute(1);
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

                List<GameAssetBundleManifest> runtimeBundleManifests = new List<GameAssetBundleManifest>();
                for (int i = 0; i < options.Length; i++)
                {
                    GameResourceModuleManifest gameResourceModuleManifest = ResourceManager.instance.GetRuntimeModuleManifest(options[i].moduleName);
                    if (gameResourceModuleManifest is null)
                    {
                        Engine.Console.Log("获取资源模块信息失败，请确认在加载模块前已执行了模块更新检查", options[i].moduleName);
                        break;
                    }

                    if (gameResourceModuleManifest.bundleList is null || gameResourceModuleManifest.bundleList.Count is 0)
                    {
                        status = Status.Failed;
                        Engine.Console.Error("Not Find Bundle List:", options[i].moduleName);
                        continue;
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

                IRequestAssetBundleScheduleHandle[] requestAssetBundleExecuteHandles = new IRequestAssetBundleScheduleHandle[runtimeBundleManifests.Count];
                //todo 开始加载资源包
                for (int i = 0; i < runtimeBundleManifests.Count; i++)
                {
                    requestAssetBundleExecuteHandles[i] = IRequestAssetBundleScheduleHandle.Create(runtimeBundleManifests[i]);
                    requestAssetBundleExecuteHandles[i].Execute();
                }

                yield return WaitFor.Create(() =>
                {
                    int count = requestAssetBundleExecuteHandles.Where(x => x.status == Status.Failed || x.status == Status.Success).Count();
                    this.subscribe?.Execute((float)count / (float)requestAssetBundleExecuteHandles.Length);
                    return count == requestAssetBundleExecuteHandles.Length;
                });
                foreach (var VARIABLE in requestAssetBundleExecuteHandles)
                {
                    VARIABLE.Dispose();
                }

                status = Status.Success;
            }
        }
    }
}