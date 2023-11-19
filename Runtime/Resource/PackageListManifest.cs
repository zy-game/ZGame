using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace ZGame.Resource
{
    public class PackageListManifest
    {
        public string name;
        public uint version;
        public PackageManifest[] packages;

        private static List<PackageListManifest> _manifests = new List<PackageListManifest>();

        public static async UniTask<PackageListManifest> Find(string name)
        {
            PackageListManifest packageListManifest = _manifests.Find(x => x.name == name);
            if (packageListManifest is null)
            {
                packageListManifest = await Engine.Network.Get<PackageListManifest>(Engine.Resource.GetNetworkResourceUrl(name + ".ini"));
                if (packageListManifest is null)
                {
                    return default;
                }

                _manifests.Add(packageListManifest);
            }

            return packageListManifest;
        }
    }

    [Serializable]
    public class PackageManifest
    {
        public string name;
        public uint version;
        public string parent;
        public string[] files;
        public Dependencies[] dependencies;
    }

    [Serializable]
    public class Dependencies
    {
        public string name;
        public uint version;
        public string parent;
    }
}