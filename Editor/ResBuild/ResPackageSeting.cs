using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using ZGame.Editor.ResBuild.Config;
using Object = UnityEngine.Object;

namespace ZGame.Editor.ResBuild
{
    [Options(typeof(PackageSeting))]
    [BindScene("包管理", typeof(ResBuilder))]
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

                foreach (var VARIABLE in ruler.folder)
                {
                    if (VARIABLE == null)
                    {
                        continue;
                    }


                    string path = AssetDatabase.GetAssetPath(VARIABLE);
                    string[] files = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories);

                    foreach (var VARIABLE2 in files)
                    {
                        string ex = Path.GetExtension(VARIABLE2);
                        if (ruler.exs.Contains(ex) || ex == ".meta")
                        {
                            continue;
                        }

                        ruler.exs.Add(ex);
                    }
                }
            }
        }

        public override void OnGUI()
        {
            EditorGUI.BeginChangeCheck();
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button(String.Empty, ZStyle.GUI_STYLE_ADD_BUTTON))
            {
                BuilderConfig.instance.packages.Add(new PackageSeting()
                {
                    name = "New Package Seting",
                    folder = new List<Object>(),
                    buildType = BuildType.AssetType,
                    use = true,
                    describe = String.Empty,
                    exs = new List<string>(),
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
            {
                package.use = EditorGUILayout.Toggle("是否激活规则", package.use);
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("", ZStyle.GUI_STYLE_MINUS))
                {
                    BuilderConfig.instance.packages.Remove(package);
                }

                GUILayout.EndHorizontal();
            }
            EditorGUI.BeginDisabledGroup(!package.use);
            {
                package.name = EditorGUILayout.TextField("规则名称", package.name);
                package.describe = EditorGUILayout.TextField("描述", package.describe);


                EditorGUI.BeginChangeCheck();
                {
                    if (package.folder == null)
                    {
                        package.folder = new List<Object>();
                    }

                    GUILayout.BeginVertical(EditorStyles.helpBox);
                    {
                        GUILayout.BeginHorizontal();
                        {
                            GUILayout.FlexibleSpace();
                            if (GUILayout.Button(String.Empty, ZStyle.GUI_STYLE_ADD_BUTTON))
                            {
                                package.folder.Add(null);
                            }

                            GUILayout.EndHorizontal();
                        }
                        for (int i = 0; i < package.folder.Count; i++)
                        {
                            GUILayout.BeginHorizontal();
                            package.folder[i] = EditorGUILayout.ObjectField(package.folder[i], typeof(DefaultAsset), false);
                            GUILayout.BeginHorizontal();
                            {
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
                            }
                            GUILayout.FlexibleSpace();
                            if (GUILayout.Button(String.Empty, ZStyle.GUI_STYLE_MINUS))
                            {
                                package.folder.RemoveAt(i);
                                WindowDocker.Refresh();
                            }

                            GUILayout.EndHorizontal();
                        }

                        GUILayout.EndVertical();
                    }

                    if (EditorGUI.EndChangeCheck())
                    {
                        OnEnable();
                    }
                }

                EditorGUI.EndDisabledGroup();
            }
        }
    }
}