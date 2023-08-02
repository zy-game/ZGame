using System;
using UnityEngine;

namespace ZEngine.Resource
{
    public class ResourceUpdateOptions : IReference
    {
        internal URLOptions url;
        internal string moduleName;
        internal VersionOptions version;

        public void Release()
        {
            url = null;
            version = null;
            moduleName = String.Empty;
        }

        public static ResourceUpdateOptions Create(string moduleName, URLOptions url, VersionOptions version)
        {
            ResourceUpdateOptions resourceUpdateOptions = Engine.Class.Loader<ResourceUpdateOptions>();
            resourceUpdateOptions.url = url;
            resourceUpdateOptions.version = version;
            resourceUpdateOptions.moduleName = moduleName;
            return resourceUpdateOptions;
        }
    }
}