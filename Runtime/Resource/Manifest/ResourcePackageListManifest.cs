using System.Collections.Generic;

namespace ZGame.Resource
{
    public class ResourcePackageListManifest
    {
        public string name;
        public uint version;
        public List<string> dependencies;
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
    }
}