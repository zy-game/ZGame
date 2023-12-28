using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using ZGame.FileSystem;
using ZGame.Networking;

namespace ZGame.Resource
{
    public class ResourcePackageListManifest
    {
        public string name;
        public uint version;
        public ResourcePackageManifest[] packages;

        public bool Contains(string name)
        {
            return GetPackageManifest(name) is not null;
        }

        public ResourcePackageManifest GetPackageManifest(string name)
        {
            for (int i = 0; i < packages.Length; i++)
            {
                if (packages[i].name == name)
                {
                    return packages[i];
                }
            }

            return default;
        }


        private static List<ResourcePackageListManifest> _manifests = new List<ResourcePackageListManifest>();

        public static async UniTask<ResourcePackageListManifest> GetResourcePackageListManifestConfig(string name)
        {
            ResourcePackageListManifest resourcePackageListManifest = _manifests.Find(x => x.name == name);
            if (resourcePackageListManifest is null)
            {
                resourcePackageListManifest = await NetworkManager.Get<ResourcePackageListManifest>(GlobalConfig.GetNetworkResourceUrl(name + ".ini"));
                if (resourcePackageListManifest is null)
                {
                    return default;
                }

                _manifests.Add(resourcePackageListManifest);
            }

            return resourcePackageListManifest;
        }

        public static uint GetResourcePackageVersion(string packageName)
        {
            foreach (var VARIABLE in _manifests)
            {
                ResourcePackageManifest resourcePackageManifest = VARIABLE.GetPackageManifest(packageName);
                if (resourcePackageManifest is not null)
                {
                    return resourcePackageManifest.version;
                }
            }

            return 0;
        }

        public static ResourcePackageManifest GetResourcePackageManifestWithAssetName(string assetName)
        {
            foreach (var VARIABLE in _manifests)
            {
                var m = VARIABLE.packages.FirstOrDefault(x => x.files.Contains(assetName));
                if (m is not null)
                {
                    return m;
                }
            }

            return default;
        }

        public static async UniTask<List<ResourcePackageManifest>> GetPackageList(string name)
        {
            List<ResourcePackageManifest> needUpdatePackages = new List<ResourcePackageManifest>();
            ResourcePackageListManifest resourcePackageListManifest = await GetResourcePackageListManifestConfig(name);
            if (resourcePackageListManifest is null)
            {
                return needUpdatePackages;
            }

            needUpdatePackages.AddRange(resourcePackageListManifest.packages);
            foreach (var VARIABLE in resourcePackageListManifest.packages)
            {
                if (VARIABLE.owner.Equals(resourcePackageListManifest.name))
                {
                    continue;
                }

                needUpdatePackages.AddRange(await GetPackageList(VARIABLE.owner));
            }

            return needUpdatePackages;
        }

        public static async UniTask<List<ResourcePackageManifest>> CheckNeedUpdatePackageList(string name)
        {
            List<ResourcePackageManifest> needUpdatePackages = new List<ResourcePackageManifest>();
            ResourcePackageListManifest resourcePackageListManifest = await GetResourcePackageListManifestConfig(name);
            if (resourcePackageListManifest is null)
            {
                return needUpdatePackages;
            }

            foreach (var package in resourcePackageListManifest.packages)
            {
                if (VFSManager.instance.Exist(package.name, package.version) is false && needUpdatePackages.Contains(package) is false)
                {
                    needUpdatePackages.Add(package);
                }

                foreach (var dependenc in package.dependencies)
                {
                    if (dependenc.owner.Equals(resourcePackageListManifest.name))
                    {
                        continue;
                    }

                    needUpdatePackages.AddRange(await CheckNeedUpdatePackageList(dependenc.owner));
                }
            }

            return needUpdatePackages;
        }
    }
}