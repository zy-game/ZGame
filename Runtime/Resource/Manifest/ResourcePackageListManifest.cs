using System.Collections.Generic;
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

        private static List<ResourcePackageListManifest> _manifests = new List<ResourcePackageListManifest>();

        public static async UniTask<ResourcePackageListManifest> Find(string name)
        {
            ResourcePackageListManifest resourcePackageListManifest = _manifests.Find(x => x.name == name);
            if (resourcePackageListManifest is null)
            {
                resourcePackageListManifest = await NetworkManager.instance.Get<ResourcePackageListManifest>(GameSeting.GetNetworkResourceUrl(name + ".ini"));
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

        public static async UniTask<List<ResourcePackageManifest>> CheckNeedUpdatePackageList(string name)
        {
            List<ResourcePackageManifest> needUpdatePackages = new List<ResourcePackageManifest>();
            ResourcePackageListManifest resourcePackageListManifest = await Find(name);
            if (resourcePackageListManifest is null)
                return needUpdatePackages;

            foreach (var package in resourcePackageListManifest.packages)
            {
                if (FileManager.instance.Exist(package.name, package.version) is false || needUpdatePackages.Contains(package))
                {
                    needUpdatePackages.Add(package);
                }

                foreach (var dependenc in package.dependencies)
                {
                    if (dependenc.owner.Equals(resourcePackageListManifest.name) is false)
                    {
                        needUpdatePackages.AddRange(await CheckNeedUpdatePackageList(dependenc.owner));
                        continue;
                    }

                    ResourcePackageManifest resourcePackageManifest = resourcePackageListManifest.GetPackageManifest(dependenc.name);
                    if (resourcePackageManifest is null || FileManager.instance.Exist(dependenc.name, dependenc.version) || needUpdatePackages.Contains(package))
                    {
                        continue;
                    }

                    needUpdatePackages.Add(resourcePackageManifest);
                }
            }

            return needUpdatePackages;
        }

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
    }
}