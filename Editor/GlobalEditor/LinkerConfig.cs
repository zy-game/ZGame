using System;
using System.Collections.Generic;

namespace ZGame.Editor.LinkerEditor
{
    public class LinkerConfig : SingletonScriptableObject<LinkerConfig>
    {
        public List<LinkAssembly> assemblies;

        public override void OnAwake()
        {
            if (assemblies is null)
            {
                assemblies = new List<LinkAssembly>();
            }
        }
    }

    [Serializable]
    public class LinkAssembly
    {
        public LinkNameSpace[] namespaces;
    }

    [Serializable]
    public class LinkNameSpace
    {
        public LinkAssemblyClass[] classes;
    }

    [Serializable]
    public class LinkAssemblyClass
    {
        public bool isOn;
        public string name;
        public bool isAll;
        public LinkAssemblyMethod[] method;
    }

    [Serializable]
    public class LinkAssemblyMethod
    {
        public string name;
        public bool isOn;
    }
}