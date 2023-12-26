using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

namespace ZGame.Editor.ResBuild.Config
{
    [ResourceReference("Assets/Settings/BuilderConfig.asset")]
    public class BuilderConfig : SingletonScriptableObject<BuilderConfig>
    {
        public BuildTarget target;
        public string fileExtension;
        public List<OSSOptions> ossList;
        public bool useActiveTarget = true;
        public List<PackageSeting> packages;
        public BuildAssetBundleOptions comperss;

        public static string output
        {
            get
            {
                string path = Application.dataPath + "/../output/";
                if (Directory.Exists(path) == false)
                {
                    Directory.CreateDirectory(path);
                }

                return path;
            }
        }

        public override void OnAwake()
        {
            ossList = ossList ?? new();
            packages = packages ?? new();
        }
    }
}