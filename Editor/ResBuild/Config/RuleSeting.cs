using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ZGame.Editor.ResBuild.Config
{
    public enum SpiltPackageType : byte
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

    public class RuleSeting : ConfigBase
    {
        [SerializeField] public List<RulerInfoItem> rulers;
    }

    [Serializable]
    public class RulerInfoItem
    {
        [NonSerialized] public bool selection;
        public bool use;
        public string name;
        public string describe;
        public Object folder;
        public string ignore;
        public SpiltPackageType spiltPackageType;

    }
}