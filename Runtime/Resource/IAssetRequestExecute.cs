using UnityEngine;

namespace ZEngine.Resource
{
    /// <summary>
    /// 资源加载
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IAssetRequestExecute<T> : IAssetRequestResult<T>, IExecute<T> where T : Object
    {
    }
}