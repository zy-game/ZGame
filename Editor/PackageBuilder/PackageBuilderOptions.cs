using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using ZGame.Editor.ResBuild.Config;

namespace ZGame.Editor.ResBuild
{
    public class PackageBuilderOptions
    {
        public PackageSeting seting { get; }
        public AssetBundleBuild[] builds { get; }

        public PackageBuilderOptions(PackageSeting seting, AssetBundleBuild[] builds)
        {
            this.seting = seting;
            this.builds = builds;
        }
    }
}