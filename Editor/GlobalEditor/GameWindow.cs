using System.Linq;
using HybridCLR.Editor.Settings;
using Newtonsoft.Json;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using ZGame.Config;
using ZGame.Editor.ResBuild.Config;

namespace ZGame.Editor
{
    [SubPageSetting("游戏设置", typeof(GlobalWindow))]
    public class GameWindow : SubPage
    {
        public override void SearchRightDrawing()
        {
            if (GUILayout.Button(EditorGUIUtility.IconContent(ZStyle.ADD_BUTTON_ICON), ZStyle.HEADER_BUTTON_STYLE))
            {
                BasicConfig.instance.entries.Add(new EntryConfig());
            }
        }

        public override void DrawingFoldoutHeaderRight(object userData)
        {
            if (GUILayout.Button(EditorGUIUtility.IconContent(ZStyle.DELETE_BUTTON_ICON), ZStyle.HEADER_BUTTON_STYLE))
            {
                BasicConfig.instance.entries.Remove((EntryConfig)userData);
            }
        }

        public override void OnGUI()
        {
            for (int i = 0; i < BasicConfig.instance.entries.Count; i++)
            {
                EntryConfig config = BasicConfig.instance.entries[i];
                config.isOn = OnBeginHeader(config.title, config.isOn, config);
                if (config.isOn)
                {
                    EditorGUI.BeginChangeCheck();
                    OnShowGameConfig(config);
                    if (EditorGUI.EndChangeCheck())
                    {
                        BasicConfig.OnSave();
                    }
                }
            }
        }

        private void OnShowGameConfig(EntryConfig config)
        {
            if (config.references is null)
            {
                config.references = new();
            }

            GUILayout.BeginVertical(EditorStyles.helpBox);

            config.title = EditorGUILayout.TextField("游戏名", config.title);
            config.language = (LanguageDefine)EditorGUILayout.EnumPopup("语言", config.language);
            if (config.path.IsNullOrEmpty() is false && config.assembly == null)
            {
                config.assembly = AssetDatabase.LoadAssetAtPath<AssemblyDefinitionAsset>(config.path);
            }

            CodeMode mode = (CodeMode)EditorGUILayout.EnumPopup("模式", config.mode);
            if (config.mode != mode)
            {
                config.mode = mode;
                var hotList = BasicConfig.instance.entries.Where(x => x.mode == CodeMode.Hotfix).Select(x => x.assembly).ToArray();
                HybridCLRSettings.Instance.hotUpdateAssemblyDefinitions = hotList;
                HybridCLRSettings.Instance.enable = hotList.Length > 0;
                HybridCLRSettings.Save();
            }

            var resList = BuilderConfig.instance.packages.Select(x => x.name).ToList();
            int last = resList.FindIndex(x => x == config.module);
            int curIndex = EditorGUILayout.Popup("资源包", last, resList.ToArray());
            if (curIndex >= 0 && curIndex < resList.Count)
            {
                config.module = resList[curIndex];
            }


            config.assembly = (AssemblyDefinitionAsset)EditorGUILayout.ObjectField("Assembly", config.assembly, typeof(AssemblyDefinitionAsset), false);
            GUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Reference Assembly", EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();
            if (GUILayout.Button(EditorGUIUtility.IconContent(ZStyle.REFRESH_BUTTON_ICON), ZStyle.HEADER_BUTTON_STYLE))
            {
                AssemlyInfo info = JsonConvert.DeserializeObject<AssemlyInfo>(config.assembly.text);
                config.references = info.GetReferenceList();
            }

            if (config.assembly != null)
            {
                config.entryName = config.assembly.name;
            }

            if (GUILayout.Button(EditorGUIUtility.IconContent(ZStyle.ADD_BUTTON_ICON), ZStyle.HEADER_BUTTON_STYLE))
            {
                config.references.Add(string.Empty);
            }

            GUILayout.EndHorizontal();
            GUILayout.Space(5);
            for (int j = config.references.Count - 1; j >= 0; j--)
            {
                GUILayout.BeginHorizontal(ZStyle.ITEM_BACKGROUND_STYLE);
                GUILayout.Label(config.references[j]);
                GUILayout.FlexibleSpace();
                if (GUILayout.Button(EditorGUIUtility.IconContent(ZStyle.DELETE_BUTTON_ICON), ZStyle.HEADER_BUTTON_STYLE))
                {
                    config.references.RemoveAt(j);
                }

                GUILayout.EndHorizontal();
            }

            GUILayout.EndVertical();
            GUILayout.EndVertical();
        }
    }
}