using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace ZGame.Editor.ResBuild.Config
{
    [CreateAssetMenu(menuName = "ZGame/Create BuilderConfig", fileName = "BuilderConfig", order = 0)]
    public class BuilderConfig : ScriptableObject
    {
        public RuleSeting ruleSeting;
        public UploadSeting uploadSeting;
        public BuildAssetBundleOptions comperss;
        public bool useActiveTarget = true;
        public BuildTarget target;
        public string output;
        public string ex;
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
            if (_instance != null)
            {
                return;
            }

            _instance = ScriptableObject.CreateInstance<BuilderConfig>();
            _instance.ruleSeting = ScriptableObject.CreateInstance<RuleSeting>();
            _instance.uploadSeting = ScriptableObject.CreateInstance<UploadSeting>();
            _instance.ruleSeting.name = "Ruler Setting";
            _instance.uploadSeting.name = "Upload Setting";
            AssetDatabase.CreateAsset(_instance, $"Assets/Settings/BuilderConfig.asset");
            Saved();
        }

        public static void Saved()
        {
            EditorUtility.SetDirty(_instance.ruleSeting);
            EditorUtility.SetDirty(_instance.uploadSeting);
            EditorUtility.SetDirty(_instance);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}