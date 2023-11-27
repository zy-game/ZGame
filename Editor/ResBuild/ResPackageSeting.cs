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
                    if (rulerData.folder == null)
                    {
                        continue;
                    }

                    if (rulerData.exs is null)
                    {
                        rulerData.exs = new List<ExtensionSetting>();
                    }
                    string path = AssetDatabase.GetAssetPath(rulerData.folder);
                    string[] files = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories);
                    foreach (var VARIABLE2 in files)
                    {
                        string ex = Path.GetExtension(VARIABLE2);
                        if (rulerData.exs.Find(x => x.name == ex) != null)
                        {
                            continue;
                        }

                        rulerData.exs.Add(new ExtensionSetting()
                        {
                            name = ex,
                            use = false,
                        });
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
                    items = new List<RulerData>(),
                    use = true,
                    describe = String.Empty,
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
                                    exs = new List<ExtensionSetting>(),
                                    buildType = BuildType.Once
                                });
                            }

                            GUILayout.EndHorizontal();
                        }
                        for (int i = 0; i < package.items.Count; i++)
                        {
                            RulerData rulerData = package.items[i];
                            GUILayout.BeginHorizontal();
                            rulerData.folder = EditorGUILayout.ObjectField(rulerData.folder, typeof(DefaultAsset), false);
                            GUILayout.BeginHorizontal();
                            {
                                rulerData.buildType = (BuildType)EditorGUILayout.EnumPopup("分包规则", rulerData.buildType);
                                if (rulerData.folder != null && rulerData.buildType == BuildType.AssetType)
                                {
                                    GUILayout.EndHorizontal();
                                    continue;
                                }

                                if (GUILayout.Button(rulerData.GetExtensionInfo(), EditorStyles.popup))
                                {
                                    GenericMenu menu = new GenericMenu();
                                    menu.AddItem(new GUIContent("Noting"), rulerData.exs.Where(x => x.use).Count() == 0, () => { rulerData.exs.ForEach(x => x.use = false); });
                                    menu.AddItem(new GUIContent("Everything"), rulerData.exs.Where(x => x.use).Count() == rulerData.exs.Count, () => { rulerData.exs.ForEach(x => x.use = true); });
                                    foreach (var VARIABLE in rulerData.exs)
                                    {
                                        menu.AddItem(new GUIContent(VARIABLE.name), VARIABLE.use, () => { VARIABLE.use = !VARIABLE.use; });
                                    }

                                    menu.ShowAsContext();
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