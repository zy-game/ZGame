using System.IO;
using UnityEditor;
using UnityEngine;

namespace ZGame.Editor.ResBuild.Config
{
    [CreateAssetMenu(menuName = "ZGame/Create BuilderConfig", fileName = "BuilderConfig", order = 0)]
    public class BuilderConfig : ConfigBase
    {
        public RuleSeting ruleSeting;
        public UploadSeting uploadSeting;
        public BuildAssetBundleOptions comperss;
        public bool useActiveTarget = true;
        public BuildTarget target;
        public string output;
        public string ex;
        private const string config_path = "Assets/Settings";

        public void Save()
        {
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        public static BuilderConfig Create()
        {
            if (Directory.Exists(config_path) is false)
            {
                Directory.CreateDirectory(config_path);
            }

            BuilderConfig b = new BuilderConfig();
            AssetDatabase.CreateAsset(b, config_path + "/BuilderConfig.asset");
            AssetDatabase.AddObjectToAsset(b.ruleSeting = ScriptableObject.CreateInstance<RuleSeting>(), b);
            AssetDatabase.AddObjectToAsset(b.uploadSeting = ScriptableObject.CreateInstance<UploadSeting>(), b);
            b.ruleSeting.name = "RuleSeting";
            b.uploadSeting.name = "UploadSeting";
            b.Save();
            return b;
        }
    }
}