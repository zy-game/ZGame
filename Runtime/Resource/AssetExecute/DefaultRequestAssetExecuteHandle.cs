using System.Collections;
using UnityEngine;

namespace ZEngine.Resource
{
    /// <summary>
    /// 资源加载
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IRequestAssetExecuteHandle<T> : IRequestAssetExecuteResult<T>, IExecuteHandle<IRequestAssetExecuteHandle<T>> where T : Object
    {
    }

    class DefaultRequestAssetExecuteHandle<T> : AbstractExecuteHandle, IRequestAssetExecuteResult<T>, IRequestAssetExecuteHandle<T> where T : Object
    {
        public T asset => result.asset;
        public string path => result.path;
        private InternalRequestAssetExecuteResult<T> result { get; set; }

        protected override IEnumerator ExecuteCoroutine(params object[] paramsList)
        {
            result = Engine.Class.Loader<InternalRequestAssetExecuteResult<T>>();
            result.path = paramsList[0].ToString();
            RuntimeBundleManifest manifest = ResourceManager.instance.GetBundleManifestWithAssetPath(result.path);
            if (manifest is null)
            {
                Engine.Console.Error("Not Find The Asset Bundle Manifest");
                status = Status.Failed;
                yield break;
            }

            InternalRuntimeBundleHandle runtimeAssetBundleHandle = ResourceManager.instance.GetRuntimeAssetBundleHandle(manifest.owner, manifest.name);
            if (runtimeAssetBundleHandle is null)
            {
                Engine.Console.Error($"Not find the asset bundle:{manifest.name}.please check your is loaded the bundle");
                status = Status.Failed;
                yield break;
            }

            yield return runtimeAssetBundleHandle.LoadAsync<T>(result.path, ISubscribeHandle.Create<T>(args => result.asset = args));
            status = Status.Success;
        }

        public void BindTo(GameObject gameObject)
        {
            result.BindTo(gameObject);
        }

        public void Free()
        {
            result.Free();
        }
    }
}