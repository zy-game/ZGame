using System.Collections;
using System.Collections.Generic;
using ZEngine.Options;

namespace ZEngine.Resource
{
    class DefaultResourcePreloadExecuteHandle : IResourcePreloadExecuteHandle
    {
        public Status status { get; set; }
        private IDialogHandle<Switch> dialog;
        private ISubscribeHandle<float> progress;
        private List<ISubscribeHandle> complete = new List<ISubscribeHandle>();

        public void Subscribe(ISubscribeHandle subscribe)
        {
            throw new System.NotImplementedException();
        }

        public IEnumerator ExecuteComplete()
        {
            throw new System.NotImplementedException();
        }

        public void OnPorgressChange(ISubscribeHandle<float> subscribe)
        {
            throw new System.NotImplementedException();
        }

        public void OnDialog(IDialogHandle<Switch> dialog)
        {
            throw new System.NotImplementedException();
        }

        public void Release()
        {
            throw new System.NotImplementedException();
        }

        public void Execute(params object[] paramsList)
        {
            status = Status.Execute;
            OnStart().StartCoroutine();
        }

        private IEnumerator OnStart()
        {
            if (HotfixOptions.instance.preloads is null || HotfixOptions.instance.preloads.Count is 0)
            {
                complete.ForEach(x => x.Execute(this));
                status = Status.Success;
                yield break;
            }

            List<RuntimeBundleManifest> runtimeBundleManifests = new List<RuntimeBundleManifest>();
            for (int i = 0; i < HotfixOptions.instance.preloads.Count; i++)
            {
                PreloadOptions options = HotfixOptions.instance.preloads[i];
                RuntimeModuleManifest runtimeModuleManifest = ResourceManager.instance.GetRuntimeModuleManifest(options.moduleName);
                if (runtimeModuleManifest is null)
                {
                    break;
                }

                DefaultCheckUpdateExecuteHandle defaultCheckUpdateExecuteHandle = Engine.Class.Loader<DefaultCheckUpdateExecuteHandle>();
                defaultCheckUpdateExecuteHandle.Execute(runtimeModuleManifest);
                yield return defaultCheckUpdateExecuteHandle.ExecuteComplete();
                if (defaultCheckUpdateExecuteHandle.status is not Status.Success)
                {
                    break;
                }
            }
        }
    }
}