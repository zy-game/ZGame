using System;
using System.Collections.Generic;
using UnityEngine;
using ZEngine.VFS;

namespace ZEngine.Resource
{
    class DefaultAssetBundleRequestExecute : IAssetBundleRequestExecute
    {
        public string name { get; set; }
        public string path { get; set; }
        public string module { get; set; }
        public VersionOptions version { get; set; }
        public IRuntimeBundleManifest result { get; set; }

        public IRuntimeBundleManifest Execute(params object[] args)
        {
            BundleManifest manifest = (BundleManifest)args[0];
            name = manifest.name;
            module = manifest.owner;
            version = manifest.version;
            path = VFSManager.GetLocalFilePath(name);
            BundleManifest[] manifests = GetDependenciesList(manifest);
            if (manifests is null || manifests.Length is 0)
            {
                return default;
            }

            bool success = true;
            Dictionary<BundleManifest, AssetBundle> map = new Dictionary<BundleManifest, AssetBundle>();
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

                IRuntimeBundleManifest runtimeBundleManifest = RuntimeAssetBundleHandle.Create(VARIABLE.Key, VARIABLE.Value);
                ResourceManager.instance.AddAssetBundleHandle(runtimeBundleManifest);
                if (manifest == VARIABLE.Key)
                {
                    result = runtimeBundleManifest;
                }
            }

            return result;
        }

        private void LoadDependenciesBundle(string[] dependencies)
        {
        }

        private BundleManifest[] GetDependenciesList(BundleManifest manifest)
        {
            List<BundleManifest> list = new List<BundleManifest>() { manifest };
            if (manifest.dependencies is null || manifest.dependencies.Count is 0)
            {
                return list.ToArray();
            }

            for (int i = 0; i < manifest.dependencies.Count; i++)
            {
                BundleManifest bundleManifest = ResourceManager.instance.GetResourceBundleManifest(manifest.dependencies[i]);
                if (bundleManifest is null)
                {
                    Engine.Console.Error("Not Find AssetBundle Dependencies:" + manifest.dependencies[i]);
                    return default;
                }

                BundleManifest[] manifests = GetDependenciesList(bundleManifest);
                foreach (var target in manifests)
                {
                    if (list.Contains(target))
                    {
                        continue;
                    }

                    list.Add(target);
                }
            }

            return list.ToArray();
        }

        public void Release()
        {
            result = null;
            name = String.Empty;
            path = String.Empty;
            module = String.Empty;
            version = null;
        }
    }
}