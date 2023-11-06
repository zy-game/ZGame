using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace ZGame.Resource
{
    public class ResourceModuleManifest : ScriptableObject
    {
        public string name;
        public int version;
        public ResourcePackageManifest[] packages;

        private static List<ResourceModuleManifest> _manifests = new List<ResourceModuleManifest>();

        public static async UniTask<ResourceModuleManifest> Find(string name)
        {
            ResourceModuleManifest manifest = _manifests.Find(x => x.name == name);
            if (manifest is null)
            {
                manifest = await Engine.Network.Get<ResourceModuleManifest>(Engine.Resource.GetNetworkResourceUrl(name + ".ini"));
                if (manifest is null)
                {
                    return default;
                }

                _manifests.Add(manifest);
            }

            return manifest;
        }
    }


    [Serializable]
    public class ResourcePackageManifest
    {
        public string name;
        public int version;
        public ResObjectInfo[] files;
        public PackageDependencies[] dependencies;
    }

    [Serializable]
    public class PackageDependencies
    {
        public string name;
        public int version;
    }

    [Serializable]
    public class ResObjectInfo
    {
        private string path;
        public string guid;
        public string name;
    }
}