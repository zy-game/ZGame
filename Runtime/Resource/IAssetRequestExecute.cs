using UnityEngine;

namespace ZEngine.Resource
{
    public interface IAssetRequestResult<T> : IReference
    {
        T result { get; }
        string path { get; }
        void Link(GameObject gameObject);
        void Free();
    }

    /// <summary>
    /// 资源加载
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IAssetRequestExecute<T> : IAssetRequestResult<T>, IExecute<T> where T : Object
    {
    }

    /// <summary>
    /// 资源加载
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IAssetRequestExecuteHandle<T> : IExecuteHandle<T>, IAssetRequestResult<T> where T : Object
    {
    }

    /// <summary>
    /// 场景资源加载
    /// </summary>
    public interface ISceneRequestExecuteHandle : IExecuteHandle
    {
        void OnPorgressChange(ISubscribeHandle<float> subscribe);
    }
}