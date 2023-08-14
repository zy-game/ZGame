using System;

namespace ZEngine.Resource
{
    /// <summary>
    /// 资源包加载
    /// </summary>
    public sealed class RequestAssetBundleResult : IReference
    {
        public string name;
        public string path;
        public string module;
        public VersionOptions version;
        internal InternalRuntimeBundleHandle bundle;

        public void Release()
        {
            name = String.Empty;
            path = String.Empty;
            module = String.Empty;
            version = VersionOptions.None;
            bundle = null;
        }

        internal static RequestAssetBundleResult Create(string name, string path, string module, VersionOptions ver, InternalRuntimeBundleHandle bundle)
        {
            RequestAssetBundleResult assetBundleRequestResult = Engine.Class.Loader<RequestAssetBundleResult>();
            assetBundleRequestResult.name = name;
            assetBundleRequestResult.path = path;
            assetBundleRequestResult.module = module;
            assetBundleRequestResult.version = ver;
            assetBundleRequestResult.bundle = bundle;
            return assetBundleRequestResult;
        }
    }
}