using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Serialization;
using ZGame.Window;
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
        public string name;
        public string describe;
        public List<RulerData> items;

        public static PackageSeting Create(string name, string describe = "")
        {
            return new PackageSeting()
            {
                name = name,
                describe = describe,
                items = new List<RulerData>()
            };
        }
    }

    [Serializable]
    public class RulerData
    {
        public bool use;
        public Object folder;
        public BuildType buildType;
        public Selector selector;
    }
}