using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;
using Object = UnityEngine.Object;

namespace ZGame.Config
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

    [CreateAssetMenu(menuName = "ZGame/Create Resource Builder Config", fileName = "BuilderConfig.asset", order = 1)]
    public class BuilderConfig : BaseConfig<BuilderConfig>
    {
        [TitleGroup("打包设置"), LabelText("打包当前平台")]
        public bool useActiveTarget = true;
#if UNITY_EDITOR
        [TitleGroup("打包设置"), DisableIf("useActiveTarget", true), LabelText("打包平台")]
        public UnityEditor.BuildTarget target = UnityEditor.BuildTarget.Android;

        [TitleGroup("打包设置"), LabelText("压缩方式")]
        public UnityEditor.BuildAssetBundleOptions comperss;
#endif
        [LabelText("资源包列表"), Title("资源列表")] public List<PackageSeting> packages;

        public static IEnumerable GetAllPackageNameList()
        {
            if (BuilderConfig.instance.packages is null)
            {
                return Array.Empty<string>();
            }

            return BuilderConfig.instance.packages.Select(x =>
            {
                Debug.Log(x.title);
                return x.title;
            });
        }
    }


    [Serializable]
    public class PackageSeting
    {
        [LabelText("包名"), FoldoutGroup("$title")]
        public string title;

        [LabelText("说明"), FoldoutGroup("$title")]
        public string describe;

        [ValueDropdown("GetAllPackageNameList", ExpandAllMenuItems = true), FoldoutGroup("$title")]
        public List<string> dependcies;

        [LabelText("资源包配置"), FoldoutGroup("$title")]
        public List<RulerData> items;

        public IEnumerable GetAllPackageNameList()
        {
            if (BuilderConfig.instance.packages is null)
            {
                return Array.Empty<string>();
            }

            return BuilderConfig.instance.packages.Where(x => x.title != title).Select(x => x.title);
        }
    }

    [Serializable]
    public class RulerData
    {
        // public bool use;
        [HorizontalGroup(), AssetsOnly, LabelText("目标文件夹")]
        public Object folder;

        [LabelText("打包方式"), HorizontalGroup()] public BuildType buildType;

        [ShowIf("buildType", BuildType.AssetType),
         LabelText("资源类型"),
         HorizontalGroup,
         ValueDropdown("GetAllFileExetension", IsUniqueList = true, DropdownTitle = "Select Asset Extension", DrawDropdownForListElements = false, AppendNextDrawer = false)]
        public List<string> selector;
#if UNITY_EDITOR
        IEnumerable GetAllFileExetension()
        {
            if (folder == null)
            {
                return Array.Empty<string>();
            }

            string path = UnityEditor.AssetDatabase.GetAssetPath(folder);
            return Directory.GetFiles(path, "*.*", SearchOption.AllDirectories).Select(x => Path.GetExtension(x)).Where(y => y != ".meta" && y != ".cs");
        }
#endif
    }
}