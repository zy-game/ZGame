using UnityEngine;

namespace ZEngine.Resource
{
    /// <summary>
    /// 资源包加载结果
    /// </summary>
    public interface IAssetBundleRequestResult : IReference
    {
        string name { get; }
        string path { get; }
        string module { get; }
        AssetBundle result { get; }
        VersionOptions version { get; }
    }
}