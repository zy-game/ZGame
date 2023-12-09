using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using ZGame.Editor.ResBuild.Config;
using ZGame.Window;
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
                    if (rulerData.selector is null)
                    {
                        rulerData.selector = new Selector();
                    }

                    if (rulerData.folder == null)
                    {
                        continue;
                    }

                    string path = AssetDatabase.GetAssetPath(rulerData.folder);
                    string[] files = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories);
                    foreach (var VARIABLE2 in files)
                    {
                        rulerData.selector.Add(Path.GetExtension(VARIABLE2));
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
                    EditorManager.Refresh();
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
                                selector = new Selector(),
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
                                    if (GUILayout.Button(rulerData.selector.ToString(), EditorStyles.popup, GUILayout.Width(200)))
                                    {
                                        GenericMenu menu = new GenericMenu();
                                        menu.AddItem(new GUIContent("Noting"), rulerData.selector.isNone, () => { rulerData.selector.Clear(); });
                                        menu.AddItem(new GUIContent("Everything"), rulerData.selector.isAll, () => { rulerData.selector.SelectAll(); });
                                        foreach (var VARIABLE in rulerData.selector.items)
                                        {
                                            menu.AddItem(new GUIContent(VARIABLE), rulerData.selector.IsSelected(VARIABLE), () =>
                                            {
                                                if (rulerData.selector.IsSelected(VARIABLE))
                                                {
                                                    rulerData.selector.UnSelect(VARIABLE);
                                                }
                                                else
                                                {
                                                    rulerData.selector.Select(VARIABLE);
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
                                EditorManager.Refresh();
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