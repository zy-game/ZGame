using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
            foreach (var packageSeting in BuilderConfig.instance.packages)
            {
                if (packageSeting.items == null)
                {
                    continue;
                }

                foreach (var rulerData in packageSeting.items)
                {
                    if (rulerData.exs is null)
                    {
                        rulerData.exs = new ExtensionSetting();
                    }

                    if (rulerData.folder == null)
                    {
                        continue;
                    }

                    string path = AssetDatabase.GetAssetPath(rulerData.folder);
                    string[] files = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories);
                    foreach (var VARIABLE2 in files)
                    {
                        rulerData.exs.Add(Path.GetExtension(VARIABLE2));
                    }
                }
            }
        }

        public override void OnGUI()
        {
            EditorGUI.BeginChangeCheck();
            {
                GUILayout.BeginHorizontal();
                {
                    GUILayout.Label("Ruler List", EditorStyles.boldLabel);
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button(String.Empty, ZStyle.GUI_STYLE_ADD_BUTTON))
                    {
                        BuilderConfig.instance.packages.Add(PackageSeting.Create("New Package"));
                    }

                    GUILayout.EndHorizontal();
                }

                for (int i = BuilderConfig.instance.packages.Count - 1; i >= 0; i--)
                {
                    if (search.IsNullOrEmpty() is false && BuilderConfig.instance.packages[i].name.Contains(search) is false)
                    {
                        continue;
                    }

                    GUILayout.BeginVertical(EditorStyles.helpBox);
                    {
                        DrawingRuleInfo(BuilderConfig.instance.packages[i]);
                        GUILayout.EndVertical();
                    }

                    GUILayout.Space(10);
                }

                if (EditorGUI.EndChangeCheck())
                {
                    BuilderConfig.Saved();
                }
            }
        }

        private void DrawingRuleInfo(PackageSeting package)
        {
            GUILayout.BeginHorizontal();
            {
                GUILayout.FlexibleSpace();
                if (GUILayout.Button(String.Empty, ZStyle.GUI_STYLE_MINUS))
                {
                    BuilderConfig.instance.packages.Remove(package);
                    WindowDocker.Refresh();
                }

                GUILayout.EndHorizontal();
            }
            package.name = EditorGUILayout.TextField("规则名称", package.name);
            package.describe = EditorGUILayout.TextField("描述", package.describe);
            EditorGUI.BeginChangeCheck();
            {
                if (package.items == null)
                {
                    package.items = new List<RulerData>();
                }

                GUILayout.BeginVertical(EditorStyles.helpBox);
                {
                    GUILayout.BeginHorizontal();
                    {
                        GUILayout.FlexibleSpace();
                        if (GUILayout.Button(String.Empty, ZStyle.GUI_STYLE_ADD_BUTTON))
                        {
                            package.items.Add(new RulerData()
                            {
                                use = true,
                                exs = new ExtensionSetting(),
                                buildType = BuildType.Once
                            });
                        }

                        GUILayout.EndHorizontal();
                    }
                    for (int i = 0; i < package.items.Count; i++)
                    {
                        RulerData rulerData = package.items[i];
                        GUILayout.BeginHorizontal();
                        {
                            rulerData.use = EditorGUILayout.Toggle(rulerData.use, GUILayout.Width(20));
                            rulerData.folder = EditorGUILayout.ObjectField(rulerData.folder, typeof(DefaultAsset), false);
                            GUILayout.BeginHorizontal();
                            {
                                rulerData.buildType = (BuildType)EditorGUILayout.EnumPopup(rulerData.buildType, GUILayout.Width(200));
                                if (rulerData.folder != null && rulerData.buildType == BuildType.AssetType)
                                {
                                    if (GUILayout.Button(rulerData.exs.ToString(), EditorStyles.popup, GUILayout.Width(200)))
                                    {
                                        GenericMenu menu = new GenericMenu();
                                        menu.AddItem(new GUIContent("Noting"), rulerData.exs.IsNotingSelect, () => { rulerData.exs.SelectAll(false); });
                                        menu.AddItem(new GUIContent("Everything"), rulerData.exs.IsAllSelect, () => { rulerData.exs.SelectAll(true); });
                                        foreach (var VARIABLE in rulerData.exs.allList)
                                        {
                                            menu.AddItem(new GUIContent(VARIABLE), rulerData.exs.IsSelect(VARIABLE), () =>
                                            {
                                                if (rulerData.exs.IsSelect(VARIABLE))
                                                {
                                                    rulerData.exs.Unselect(VARIABLE);
                                                }
                                                else
                                                {
                                                    rulerData.exs.Select(VARIABLE);
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
                                package.items.RemoveAt(i);
                                WindowDocker.Refresh();
                            }

                            GUILayout.EndHorizontal();
                        }
                    }

                    GUILayout.EndVertical();
                }

                if (EditorGUI.EndChangeCheck())
                {
                    OnEnable();
                }
            }
        }
    }
}