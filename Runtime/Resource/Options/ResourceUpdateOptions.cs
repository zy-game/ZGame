using UnityEngine;

namespace ZEngine.Resource
{
    public class ResourceUpdateOptions : IReference
    {
        public URLOptions url;
        public string moduleName;
        public VersionOptions version;

        public void Release()
        {
        }
    }
}