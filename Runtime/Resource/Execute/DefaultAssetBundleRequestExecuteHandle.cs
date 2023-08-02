using System;
using System.Collections;
using UnityEngine;
using ZEngine.VFS;

namespace ZEngine.Resource
{
    class DefaultAssetBundleRequestExecuteHandle : IAssetBundleRequestExecuteHandle
    {
        private Status status;
        public string name { get; set; }
        public string path { get; set; }
        public string module { get; set; }
        public AssetBundle result { get; set; }
        public VersionOptions version { get; set; }
        public float progress { get; }

        public void Release()
        {
            result = null;
            name = String.Empty;
            path = String.Empty;
            module = String.Empty;
            version = null;
            status = Status.None;
        }


        public IEnumerator Execute(params object[] paramsList)
        {
            BundleManifest manifest = (BundleManifest)paramsList[0];
            name = manifest.name;
            module = manifest.owner;
            version = manifest.version;
            path = VFSManager.GetLocalFilePath(name);
            //todo 先加载依赖包
            result = AssetBundle.LoadFromFile(path);

            yield break;
        }

        public void Subscribe(ISubscribe subscribe)
        {
        }


        public void ObserverPorgress(ISubscribe<float> subscribe)
        {
        }
    }
}