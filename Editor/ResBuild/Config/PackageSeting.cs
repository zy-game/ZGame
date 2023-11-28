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
        public ExtensionSetting exs;
    }

    [Serializable]
    public class ExtensionSetting
    {
        public List<string> allList = new List<string>();
        public List<string> select = new List<string>();

        public bool IsAllSelect
        {
            get { return select.Count == allList.Count; }
        }

        public bool IsNotingSelect
        {
            get { return select.Count == 0; }
        }

        public void Add(string ex)
        {
            if (allList.Contains(ex))
            {
                return;
            }

            allList.Add(ex);
        }

        public void Remove(string ex)
        {
            if (allList.Contains(ex))
            {
                allList.Remove(ex);
            }

            if (select.Contains(ex))
            {
                select.Remove(ex);
            }
        }

        public void SelectAll(bool state)
        {
            select.Clear();
            select.AddRange(allList);
        }

        public void Select(string ex)
        {
            if (select.Contains(ex))
            {
                return;
            }

            select.Add(ex);
        }

        public void Unselect(string ex)
        {
            if (select.Contains(ex))
            {
                select.Remove(ex);
            }
        }

        public bool IsSelect(string ex)
        {
            return select.Contains(ex);
        }

        public override string ToString()
        {
            if (allList.Count == select.Count)
            {
                return "Everyting";
            }
            else if (select.Count == 0)
            {
                return "Noting";
            }
            else
            {
                return string.Join(",", select);
            }
        }
    }
}