using UnityEngine;

namespace ZEngine.Resource
{
    /// <summary>
    /// 资源加载
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IAssetRequestExecuteHandle<T> : IExecuteHandle<T>, IAssetRequestResult<T> where T : Object
    {
    }

    public interface IAssetRequestResult<T> : IReference
    {
        T result { get; }
        string path { get; }
        void LinkObject(GameObject gameObject);
        void FreeAsset();
    }
}