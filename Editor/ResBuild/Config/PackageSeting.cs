using System;
using System.Collections.Generic;
using UnityEngine.Serialization;
using Object = UnityEngine.Object;

namespace ZGame.Editor.ResBuild.Config
{
    public enum BuildType : byte
    {
        /// <summary>
        /// 将所有文件打入一个资源包中
        /// </summary>
        Once,

        /// <summary>
        /// 以单个资源为一个包
        /// </summary>
        Asset,

        /// <summary>
        /// 按资源类型打包
        /// </summary>
        AssetType,

        /// <summary>
        /// 按文件夹打包
        /// </summary>
        Folder,
    }

    [Serializable]
    public class PackageSeting
    {
        [NonSerialized] public bool selection;
        [NonSerialized] public List<string> exs;
        public bool use;
        public string name;
        public string describe;
        public Object folder;
        public BuildType buildType;
        public List<string> contentExtensionList;
    }
}