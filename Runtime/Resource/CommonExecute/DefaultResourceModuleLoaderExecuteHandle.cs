using System.Collections;
using System.Collections.Generic;
using ZEngine.Options;

namespace ZEngine.Resource
{
    /// <summary>
    /// 资源预加载
    /// </summary>
    public interface IResourceModuleLoaderExecuteHandle : IExecuteHandle<IResourceModuleLoaderExecuteHandle>
    {
    }

    class DefaultResourceModuleLoaderExecuteHandle : ExecuteHandle<IResourceModuleLoaderExecuteHandle>, IResourceModuleLoaderExecuteHandle
    {
        public override void Execute(params object[] paramsList)
        {
            status = Status.Execute;
            OnStart().StartCoroutine();
        }

        private IEnumerator OnStart()
        {
            if (HotfixOptions.instance.preloads is null || HotfixOptions.instance.preloads.Count is 0)
            {
                status = Status.Success;
                OnComplete();
                yield break;
            }

            List<RuntimeBundleManifest> runtimeBundleManifests = new List<RuntimeBundleManifest>();
            for (int i = 0; i < HotfixOptions.instance.preloads.Count; i++)
            {
                PreloadOptions options = HotfixOptions.instance.preloads[i];
                RuntimeModuleManifest runtimeModuleManifest = ResourceManager.instance.GetRuntimeModuleManifest(options.moduleName);
                if (runtimeModuleManifest is null)
                {
                    Engine.Console.Log("获取资源模块信息失败，请确认在加载模块前已执行了模块更新检查", options.moduleName);
                    break;
                }

                if (runtimeModuleManifest.bundleList is null || runtimeModuleManifest.bundleList.Count is 0)
                {
                    status = Status.Failed;
                    Engine.Console.Error("Not Find Bundle List:", options.moduleName);
                    continue;
                }

                runtimeBundleManifests.AddRange(runtimeModuleManifest.bundleList);
            }

            //todo 开始加载资源包
            foreach (var VARIABLE in runtimeBundleManifests)
            {
                DefaultRequestAssetBundleExecuteHandle defaultRequestAssetBundleExecuteHandle = Engine.Class.Loader<DefaultRequestAssetBundleExecuteHandle>();
                defaultRequestAssetBundleExecuteHandle.Execute(VARIABLE);
            }

            status = Status.Success;
            OnComplete();
        }
    }
}