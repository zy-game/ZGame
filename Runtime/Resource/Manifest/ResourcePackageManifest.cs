using System;

namespace ZGame.Resource
{
    [Serializable]
    public class ResourcePackageManifest
    {
        public string name;
        public uint version;
        public string owner;
        public string[] files;
        public Dependencies[] dependencies;
        
        
    }
}