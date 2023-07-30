using System;
using System.Collections.Generic;

namespace ZEngine.Resource
{
    [Serializable]
    public sealed class ResourceBundleOptions
    {
        public string buildTag;
        public string bundleName;
        public VersionOptions version;
        public List<string> dependencies;
        public List<AssetDataOptions> fileDataList;
    }

    [Serializable]
    public sealed class AssetDataOptions : IReference
    {
        public string name;
        public string guid;
        public string path;


        public void Release()
        {
        }
    }
}