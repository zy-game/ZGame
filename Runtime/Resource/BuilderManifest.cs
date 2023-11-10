using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace ZGame.Resource
{
    public class BuilderManifest
    {
        public string name;
        public uint version;
        public PackageManifest[] packages;

        private static List<BuilderManifest> _manifests = new List<BuilderManifest>();

        public static async UniTask<BuilderManifest> Find(string name)
        {
            BuilderManifest manifest = _manifests.Find(x => x.name == name);
            if (manifest is null)
            {
                manifest = await Engine.Network.Get<BuilderManifest>(Engine.Resource.GetNetworkResourceUrl(name + ".ini"));
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
    public class PackageManifest
    {
        public string name;
        public uint version;
        public string[] files;
        public Dependencies[] dependencies;
    }

    [Serializable]
    public class Dependencies
    {
        public string name;
        public uint version;
    }
}