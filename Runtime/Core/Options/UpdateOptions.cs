using System;
using UnityEngine;

namespace ZEngine.Resource
{
    public class UpdateOptions : IReference
    {
        internal URLOptions url;
        internal string moduleName;

        public void Release()
        {
            url = null;
            moduleName = String.Empty;
        }

        public static UpdateOptions Create(string moduleName, URLOptions url)
        {
            UpdateOptions resourceUpdateOptions = Engine.Class.Loader<UpdateOptions>();
            resourceUpdateOptions.url = url;
            resourceUpdateOptions.moduleName = moduleName;
            return resourceUpdateOptions;
        }
    }
}