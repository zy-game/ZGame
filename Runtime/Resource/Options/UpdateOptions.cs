using System;
using UnityEngine;

namespace ZEngine.Resource
{
    public class UpdateOptions : IReference
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

        public static UpdateOptions Create(string moduleName, URLOptions url, VersionOptions version)
        {
            UpdateOptions resourceUpdateOptions = Engine.Class.Loader<UpdateOptions>();
            resourceUpdateOptions.url = url;
            resourceUpdateOptions.version = version;
            resourceUpdateOptions.moduleName = moduleName;
            return resourceUpdateOptions;
        }
    }
}