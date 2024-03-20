using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

namespace ZGame.Editor.ResBuild.Config
{
    [ResourceReference("Assets/Settings/BuilderConfig.asset")]
    public class BuilderConfig : BaseConfig<BuilderConfig>
    {
        public BuildTarget target;
        public string fileExtension;
        public bool useActiveTarget = true;
        public List<PackageSeting> packages;
        public BuildAssetBundleOptions comperss;

        public override void OnAwake()
        {
            packages = packages ?? new();
        }
    }
}