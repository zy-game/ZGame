using System;
using UnityEngine;
using ZEngine.VFS;

namespace ZEngine.Resource
{
    class DefaultAssetBundleRequestExecute : IAssetBundleRequestExecute
    {
        private Status status;
        public string name { get; set; }
        public string path { get; set; }
        public string module { get; set; }
        public VersionOptions version { get; set; }
        public AssetBundle result { get; set; }


        public bool EnsureExecuteSuccessfuly()
        {
            return status == Status.Success;
        }

        public void Execute(params object[] args)
        {
            BundleManifest manifest = (BundleManifest)args[0];
            name = manifest.name;
            module = manifest.owner;
            version = manifest.version;
            path = VFSManager.GetLocalFilePath(name);
            //todo 先加载依赖包
            result = AssetBundle.LoadFromFile(path);
        }

        public void Release()
        {
            result = null;
            name = String.Empty;
            path = String.Empty;
            module = String.Empty;
            version = null;
            status = Status.None;
        }
    }
}