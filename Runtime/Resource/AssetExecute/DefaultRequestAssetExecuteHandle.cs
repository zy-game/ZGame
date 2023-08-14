using System.Collections;
using UnityEngine;

namespace ZEngine.Resource
{
    /// <summary>
    /// 资源加载
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IRequestAssetExecuteHandle<T> : IExecuteHandle<RequestAssetResult<T>> where T : Object
    {
    }

    class DefaultRequestAssetExecuteHandle<T> : ExecuteHandle<RequestAssetResult<T>>, IRequestAssetExecuteHandle<T> where T : Object
    {
        public override void Execute(params object[] args)
        {
            status = Status.Execute;
            string path = args[0].ToString();
            OnStart(path).StartCoroutine();
        }

        IEnumerator OnStart(string path)
        {
            RuntimeBundleManifest manifest = ResourceManager.instance.GetBundleManifestWithAssetPath(path);
            if (manifest is null)
            {
                Engine.Console.Error("Not Find The Asset Bundle Manifest");
                status = Status.Failed;
                OnComplete();
                yield break;
            }

            InternalRuntimeBundleHandle runtimeAssetBundleHandle = ResourceManager.instance.GetRuntimeAssetBundleHandle(manifest.owner, manifest.name);
            if (runtimeAssetBundleHandle is null)
            {
                Engine.Console.Error($"Not find the asset bundle:{manifest.name}.please check your is loaded the bundle");
                status = Status.Failed;
                OnComplete();
                yield break;
            }

            yield return runtimeAssetBundleHandle.LoadAsync<T>(path, ISubscribeHandle.Create<T>(args => { result = RequestAssetResult<T>.Create(path, args); }));
            status = Status.Success;
            OnComplete();
        }
    }
}