using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using ZGame.Editor.ResBuild.Config;

namespace ZGame.Editor.ResBuild
{
    public class ResBuilder : PageScene
    {
        private const string key = "__build config__";

        public override string name { get; } = "资源";
        public BuilderConfig config;


        public ResBuilder(Docker window) : base(window)
        {
            RegisterSubPageScene<ResRuleSeting>();
            RegisterSubPageScene<ResUploader>();
            if (EditorPrefs.HasKey(key))
            {
                config = AssetDatabase.LoadAssetAtPath<BuilderConfig>(EditorPrefs.GetString(key));
            }
        }

        public override PageScene OpenAssetObject(Object obj)
        {
            if (obj is BuilderConfig builderConfig)
            {
                config = builderConfig;
                EditorPrefs.SetString(key, AssetDatabase.GetAssetPath(config));
                return this;
            }

            return base.OpenAssetObject(obj);
        }

        public override void OnGUI(string search, Rect rect)
        {
            EditorGUI.BeginChangeCheck();
            GUILayout.BeginHorizontal();
            config = (BuilderConfig)EditorGUILayout.ObjectField("资源配置", config, typeof(BuilderConfig), false, GUILayout.MinWidth(400));
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Create"))
            {
                config = BuilderConfig.Create();
            }

            GUILayout.EndHorizontal();

            GUILayout.BeginVertical("Output Seting", EditorStyles.helpBox);
            GUILayout.Space(20);
            config.comperss = (BuildAssetBundleOptions)EditorGUILayout.EnumPopup("压缩方式", config.comperss);
            config.useActiveTarget = EditorGUILayout.Toggle("是否跟随激活平台", config.useActiveTarget);
            EditorGUI.BeginDisabledGroup(config.useActiveTarget);
            config.target = (BuildTarget)EditorGUILayout.EnumPopup("编译平台", config.target);
            EditorGUI.EndDisabledGroup();
            config.output = EditorGUILayout.TextField("输出路径", config.output);
            config.ex = EditorGUILayout.TextField("文件扩展名", config.ex);
            GUILayout.EndHorizontal();

            GUILayout.BeginVertical("Ruler List", EditorStyles.helpBox);
            GUILayout.Space(20);
            foreach (var VARIABLE in config.ruleSeting.rulers)
            {
                if (VARIABLE.folder == null || (search.IsNullOrEmpty() is false && VARIABLE.name.StartsWith(search) is false))
                {
                    continue;
                }

                GUILayout.BeginHorizontal();
                VARIABLE.selection = GUILayout.Toggle(VARIABLE.selection, VARIABLE.name, GUILayout.Width(300));
                GUILayout.Space(40);
                GUILayout.Label(VARIABLE.describe);
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Edit"))
                {
                    docker.SwitchPageScene(GetSubPageScene<ResRuleSeting>());
                }

                GUILayout.EndHorizontal();
                GUILayout.Box("", ZStyle.GUI_STYLE_BOX_BACKGROUND, GUILayout.Width(rect.width), GUILayout.Height(1));
                GUILayout.Space(3);
            }

            GUILayout.EndVertical();
            if (GUILayout.Button("构建资源"))
            {
                OnBuild();
            }

            if (EditorGUI.EndChangeCheck())
            {
                EditorPrefs.SetString(key, AssetDatabase.GetAssetPath(config));
            }
        }

        private void OnBuild()
        {
            List<RulerInfoItem> items = config.ruleSeting.rulers.Where(x => x.selection).ToList();
            if (items is null || items.Count == 0)
            {
                return;
            }
        }

        private void OnBuildBundle(string output, BuildTarget target, params AssetBundleBuild[] builds)
        {
            var manifest = BuildPipeline.BuildAssetBundles(output, builds, BuildAssetBundleOptions.None, target);
        }
    }
}