using UnityEngine;

namespace ZEngine.Resource
{
    /// <summary>
    /// 资源加载
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IAssetRequestExecuteHandle<T> : IExecuteHandle<IAssetRequestExecuteHandle<T>>, IAssetRequestResult<T> where T : Object
    {
    }
}