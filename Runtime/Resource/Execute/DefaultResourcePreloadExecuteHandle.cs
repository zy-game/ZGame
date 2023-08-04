using System.Collections;
using System.Collections.Generic;
using ZEngine.Options;

namespace ZEngine.Resource
{
    class DefaultResourcePreloadExecuteHandle : IResourcePreloadExecuteHandle
    {
        public Status status { get; set; }
        private IDialogHandle<Switch> dialog;
        private ISubscribeExecuteHandle<float> progress;
        private List<ISubscribeExecuteHandle> complete = new List<ISubscribeExecuteHandle>();

        public void Subscribe(ISubscribeExecuteHandle subscribe)
        {
            throw new System.NotImplementedException();
        }

        public IEnumerator Complete()
        {
            throw new System.NotImplementedException();
        }

        public void OnPorgressChange(ISubscribeExecuteHandle<float> subscribe)
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

        public IEnumerator Execute(params object[] paramsList)
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
                RuntimeModuleManifest runtimeModuleManifest = ResourceManager.instance.GetModuleManifest(options.moduleName);
                if (runtimeModuleManifest is null)
                {
                    break;
                }

                DefaultCheckUpdateExecuteHandle defaultCheckUpdateExecuteHandle = Engine.Class.Loader<DefaultCheckUpdateExecuteHandle>();
                yield return defaultCheckUpdateExecuteHandle.Execute(runtimeModuleManifest);
                if (defaultCheckUpdateExecuteHandle.status is not Status.Success)
                {
                    break;
                }
            }
        }
    }
}