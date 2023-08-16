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
    }

    class DefaultResourceModuleLoaderExecuteHandle : ExecuteHandle, IExecuteHandle<IResourceModuleLoaderExecuteHandle>, IResourceModuleLoaderExecuteHandle
    {
        private ModuleOptions[] moduleList;

        public override void Execute(params object[] paramsList)
        {
            status = Status.Execute;
            moduleList = paramsList.Cast<ModuleOptions>().ToArray();
            OnStart().StartCoroutine();
        }

        private IEnumerator OnStart()
        {
            if (moduleList is null || moduleList.Length is 0)
            {
                status = Status.Success;
                OnComplete();
                yield break;
            }

            List<RuntimeBundleManifest> runtimeBundleManifests = new List<RuntimeBundleManifest>();
            for (int i = 0; i < moduleList.Length; i++)
            {
                ModuleOptions options = moduleList[i];
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

                foreach (var UPPER in runtimeModuleManifest.bundleList)
                {
                    if (ResourceManager.instance.HasLoadAssetBundle(runtimeModuleManifest.name, UPPER.name))
                    {
                        continue;
                    }

                    runtimeBundleManifests.Add(UPPER);
                }
            }

            DefaultRequestAssetBundleExecuteHandle[] requestAssetBundleExecuteHandles = new DefaultRequestAssetBundleExecuteHandle[runtimeBundleManifests.Count];
            //todo 开始加载资源包
            for (int i = 0; i < runtimeBundleManifests.Count; i++)
            {
                requestAssetBundleExecuteHandles[i] = Engine.Class.Loader<DefaultRequestAssetBundleExecuteHandle>();
                requestAssetBundleExecuteHandles[i].Execute(runtimeBundleManifests[i]);
            }

            yield return WaitFor.Create(() =>
            {
                int count = requestAssetBundleExecuteHandles.Where(x => x.status == Status.Failed || x.status == Status.Success).Count();
                OnProgress((float)count / (float)requestAssetBundleExecuteHandles.Length);
                return count == requestAssetBundleExecuteHandles.Length;
            });

            status = Status.Success;
            OnComplete();
        }
    }
}