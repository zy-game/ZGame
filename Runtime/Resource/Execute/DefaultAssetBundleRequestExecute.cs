using System;
using System.Collections.Generic;
using UnityEngine;
using ZEngine.VFS;

namespace ZEngine.Resource
{
    class DefaultAssetBundleRequestExecute : IAssetBundleRequestExecute, IAssetBundleRequestResult
    {
        public string name { get; set; }
        public string path { get; set; }
        public string module { get; set; }
        public VersionOptions version { get; set; }
        public IRuntimeBundleHandle bundle { get; set; }


        public void Release()
        {
            bundle = null;
            name = String.Empty;
            path = String.Empty;
            module = String.Empty;
            version = null;
        }

        public IAssetBundleRequestResult Execute(params object[] args)
        {
            RuntimeBundleManifest manifest = (RuntimeBundleManifest)args[0];
            name = manifest.name;
            module = manifest.owner;
            version = manifest.version;
            path = VFSManager.GetLocalFilePath(name);
            RuntimeBundleManifest[] manifests = ResourceManager.instance.GetBundleDependenciesList(manifest);
            if (manifests is null || manifests.Length is 0)
            {
                return default;
            }

            bool success = true;
            Dictionary<RuntimeBundleManifest, AssetBundle> map = new Dictionary<RuntimeBundleManifest, AssetBundle>();
            for (int i = 0; i < manifests.Length; i++)
            {
                if (ResourceManager.instance.HasLoadAssetBundle(manifests[i].name))
                {
                    continue;
                }

                IReadFileExecute readFileExecute = Engine.FileSystem.ReadFile(manifests[i].name);
                if (readFileExecute.bytes is null || readFileExecute.bytes.Length is 0)
                {
                    success = false;
                    break;
                }

                AssetBundle assetBundle = AssetBundle.LoadFromMemory(readFileExecute.bytes);
                if (assetBundle is null)
                {
                    success = false;
                    break;
                }

                map.Add(manifests[i], assetBundle);
            }

            foreach (var VARIABLE in map)
            {
                if (success is false)
                {
                    VARIABLE.Value.Unload(true);
                    continue;
                }

                IRuntimeBundleHandle runtimeBundleManifest = RuntimeAssetBundleHandle.Create(VARIABLE.Key, VARIABLE.Value);
                ResourceManager.instance.AddAssetBundleHandle(runtimeBundleManifest);
                if (manifest == VARIABLE.Key)
                {
                    bundle = runtimeBundleManifest;
                }
            }

            return this;
        }
    }
}