using UnityEngine;

namespace ZEngine.Resource
{
    /// <summary>
    /// 资源加载结果
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IAssetRequestResult<T> : IReference where T : Object
    {
        T result { get; }
        string path { get; }
        void LinkObject(GameObject gameObject);
        void FreeAsset();
    }
}