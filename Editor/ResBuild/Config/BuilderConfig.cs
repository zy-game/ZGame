using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

namespace ZGame.Editor.ResBuild.Config
{
    [CreateAssetMenu(menuName = "ZGame/BuilderConfig", fileName = "BuilderConfig", order = 0)]
    public class BuilderConfig : ScriptableObject
    {
        public BuildTarget target;
        public string fileExtension;
        public List<OSSOptions> ossList;
        public bool useActiveTarget = true;
        public List<PackageSeting> packages;
        public BuildAssetBundleOptions comperss;
        private static BuilderConfig _instance;

        public static BuilderConfig instance
        {
            get
            {
                if (_instance == null)
                {
                    Initialized();
                }

                return _instance;
            }
        }

        private static void Initialized()
        {
            _instance = AssetDatabase.LoadAssetAtPath<BuilderConfig>("Assets/Settings/BuilderConfig.asset");
            if (_instance == null)
            {
                _instance = ScriptableObject.CreateInstance<BuilderConfig>();
                _instance.target = EditorUserBuildSettings.activeBuildTarget;
                AssetDatabase.CreateAsset(_instance, $"Assets/Settings/BuilderConfig.asset");
            }

            if (_instance.packages is null)
            {
                _instance.packages = new List<PackageSeting>();
            }

            if (_instance.ossList is null)
            {
                _instance.ossList = new List<OSSOptions>();
            }

            Saved();
        }

        public static void Saved()
        {
            EditorUtility.SetDirty(_instance);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

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
    }
}