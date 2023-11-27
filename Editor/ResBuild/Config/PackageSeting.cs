using System;
using System.Collections.Generic;
using System.Linq;
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
        public bool use;
        public string name;
        public string describe;
        public List<RulerData> items;
    }

    [Serializable]
    public class RulerData
    {
        public bool use;
        public Object folder;
        public BuildType buildType;
        public List<ExtensionSetting> exs;


        public string GetExtensionInfo()
        {
            if (exs == null || exs.Find(x => x.use == true) is null)
            {
                return "Noting";
            }

            if (exs.Find(x => x.use == false) is not null)
            {
                return "Everyting";
            }

            string result = string.Join(",", exs.FindAll(x => x.use == true).Select(x => x.name));
            if (result.Length > 100)
            {
                result = result.Substring(0, 100);
            }

            return result;
        }


        public bool IsAllExtension()
        {
            return exs.Find(x => x.use == false) is not null;
        }
    }

    [Serializable]
    public class ExtensionSetting
    {
        public bool use;
        public string name;
    }
}