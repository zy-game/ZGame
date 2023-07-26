using System;
using System.Collections.Generic;
using ZEngine.Core;

namespace ZEngine.Resource
{
    [Serializable]
    public sealed class ResourceBundleVersion : ZVersion
    {
        public string bundleName;
        public string buildTag;
        public List<AssetFileData> fileDataList;
    }

    [Serializable]
    public sealed class AssetFileData : IReference
    {
        public string name;
        public string guid;
        public string path;


        public void Release()
        {
        }
    }
}