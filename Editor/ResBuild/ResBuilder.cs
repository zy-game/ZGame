using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;
using ZGame.Editor.Command;
using ZGame.Editor.ResBuild.Config;
using ZGame.Resource;
using ZGame.Resource.Config;
using Object = UnityEngine.Object;

namespace ZGame.Editor.ResBuild
{
    [PageConfig("资源", null, false, typeof(BuilderConfig))]
    public class ResBuilder : ToolbarScene
    {
        private const string key = "__build config__";

        public override void OnGUI()
        {
            GUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Package List", EditorStyles.boldLabel);
            GUILayout.Space(5);
            foreach (var VARIABLE in BuilderConfig.instance.packages)
            {
                if (VARIABLE.items == null || (search.IsNullOrEmpty() is false && VARIABLE.name.StartsWith(search) is false))
                {
                    continue;
                }

                GUILayout.BeginHorizontal(ZStyle.ITEM_BACKGROUND_STYLE);
                VARIABLE.selection = GUILayout.Toggle(VARIABLE.selection, VARIABLE.name, GUILayout.Width(300));
                GUILayout.Space(40);
                GUILayout.Label(VARIABLE.describe);
                GUILayout.FlexibleSpace();
                if (GUILayout.Button(EditorGUIUtility.IconContent(ZStyle.SETTING_BUTTON_ICON), ZStyle.HEADER_BUTTON_STYLE))
                {
                    ToolsWindow.SwitchScene<ResPackageSetting>();
                }

                if (GUILayout.Button(EditorGUIUtility.IconContent(ZStyle.PLAY_BUTTON_ICON), ZStyle.HEADER_BUTTON_STYLE))
                {
                    ZGame.CommandManager.OnExecuteCommand<BuildPackageCommand>(VARIABLE);
                    EditorUtility.DisplayDialog("打包完成", "资源打包成功", "OK");
                }

                GUILayout.Space(2);
                GUILayout.EndHorizontal();
            }

            GUILayout.EndVertical();
            if (GUILayout.Button("构建资源"))
            {
                PackageSeting[] packageSetings = BuilderConfig.instance.packages.Where(x => x.selection).ToArray();
                ZGame.CommandManager.OnExecuteCommand<BuildPackageCommand>(packageSetings);
                EditorUtility.DisplayDialog("打包完成", "资源打包成功", "OK");
            }

            if (Event.current.type == EventType.KeyDown && Event.current.control && Event.current.keyCode == KeyCode.S)
            {
                BuilderConfig.OnSave();
            }
        }
    }
}