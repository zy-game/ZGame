using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using ZGame.Editor.ResBuild.Config;

namespace ZGame.Editor.ResBuild
{
    [Options(typeof(PackageSeting))]
    [BindScene("资源包管理", typeof(ResBuilder))]
    public class ResPackageSeting : PageScene
    {
        public override void OnEnable()
        {
            foreach (var ruler in BuilderConfig.instance.packages)
            {
                ruler.exs = new List<string>();
                if (ruler.folder == null)
                {
                    return;
                }

                string path = AssetDatabase.GetAssetPath(ruler.folder);
                string[] files = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories);

                foreach (var VARIABLE in files)
                {
                    string ex = Path.GetExtension(VARIABLE);
                    if (ruler.exs.Contains(ex) || ex == ".meta")
                    {
                        continue;
                    }

                    ruler.exs.Add(ex);
                }
            }
        }

        public override void OnGUI()
        {
            EditorGUI.BeginChangeCheck();
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Create"))
            {
                BuilderConfig.instance.packages.Add(new PackageSeting()
                {
                    name = "New Package Seting",
                    folder = null,
                    buildType = BuildType.AssetType,
                    contentExtensionList = new List<string>()
                });
            }

            GUILayout.EndHorizontal();
            for (int i = BuilderConfig.instance.packages.Count - 1; i >= 0; i--)
            {
                if (search.IsNullOrEmpty() is false && BuilderConfig.instance.packages[i].name.Contains(search) is false)
                {
                    continue;
                }

                GUILayout.BeginVertical(EditorStyles.helpBox);
                DrawingRuleInfo(BuilderConfig.instance.packages[i]);
                GUILayout.EndVertical();
                GUILayout.Space(10);
            }

            if (EditorGUI.EndChangeCheck())
            {
                BuilderConfig.Saved();
            }
        }

        private void DrawingRuleInfo(PackageSeting package)
        {
            GUILayout.BeginHorizontal();

            package.use = EditorGUILayout.Toggle("是否激活规则", package.use);
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("", ZStyle.GUI_STYLE_MINUS))
            {
                BuilderConfig.instance.packages.Remove(package);
            }

            GUILayout.EndHorizontal();
            EditorGUI.BeginDisabledGroup(!package.use);
            GUILayout.Label(package.name);
            EditorGUI.BeginChangeCheck();
            package.folder = EditorGUILayout.ObjectField("资源目录", package.folder, typeof(DefaultAsset), false);
            if (EditorGUI.EndChangeCheck())
            {
                OnEnable();
            }

            if (package.folder != null)
            {
                package.name = package.folder.name + " Ruler Seting";
            }

            GUILayout.BeginHorizontal();
            package.buildType = (BuildType)EditorGUILayout.EnumPopup("分包规则", package.buildType);
            if (package.folder != null && package.buildType == BuildType.AssetType)
            {
                string name = String.Empty;
                if (package.contentExtensionList.Count == 0)
                {
                    name = "Noting";
                }
                else
                {
                    if (package.exs.Count == package.contentExtensionList.Count)
                    {
                        name = "Everyting";
                    }
                    else
                    {
                        name = string.Join(",", package.exs);
                        if (name.Length > 20)
                        {
                            name = name.Substring(0, 25) + "...";
                        }
                    }
                }

                if (GUILayout.Button(name, EditorStyles.popup))
                {
                    GenericMenu menu = new GenericMenu();
                    menu.AddItem(new GUIContent("Noting"), package.contentExtensionList.Count == 0, () => { package.contentExtensionList.Clear(); });
                    menu.AddItem(new GUIContent("Everything"), package.exs.Count == package.contentExtensionList.Count, () =>
                    {
                        package.contentExtensionList.Clear();
                        package.contentExtensionList.AddRange(package.exs);
                    });
                    foreach (var VARIABLE in package.exs)
                    {
                        menu.AddItem(new GUIContent(VARIABLE), package.contentExtensionList.Contains(VARIABLE), () =>
                        {
                            if (package.contentExtensionList.Contains(VARIABLE))
                            {
                                package.contentExtensionList.Remove(VARIABLE);
                            }
                            else
                            {
                                package.contentExtensionList.Add(VARIABLE);
                            }
                        });
                    }

                    menu.ShowAsContext();
                }
            }

            GUILayout.EndHorizontal();
            package.describe = EditorGUILayout.TextField("描述", package.describe);

            EditorGUI.EndDisabledGroup();
        }
    }
}