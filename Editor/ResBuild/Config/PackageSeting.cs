using System;
using System.Collections.Generic;
using UnityEngine.Serialization;
using Object = UnityEngine.Object;

namespace ZGame.Editor.ResBuild.Config
{
    [Serializable]
    public class PackageSeting
    {
        [NonSerialized] public bool selection;
        [NonSerialized] public List<string> exs;
        public bool use;
        public string name;
        public string describe;
        public Object folder;
        public PackageBuildType buildType;
        public List<string> contentExtensionList;
    }
}