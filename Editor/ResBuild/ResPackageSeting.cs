using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using ZGame.Editor.Command;
using ZGame.Editor.ResBuild.Config;
using ZGame.Resource.Config;
using Object = UnityEngine.Object;

namespace ZGame.Editor.ResBuild
{
    [PageConfig("资源包管理", typeof(ResBuilder), false, typeof(PackageSeting))]
    public class ResPackageSetting : ToolbarScene
    {
        public override void OnEnable(params object[] args)
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
                    foreach (var VARIABLE in files)
                    {
                        string ex = Path.GetExtension(VARIABLE);
                        if (rulerData.selector.Contains(ex) || ex.Equals(".meta"))
                        {
                            continue;
                        }

                        rulerData.selector.Add(ex);
                    }
                }
            }
        }

        public override void SearchRightDrawing()
        {
            if (GUILayout.Button(EditorGUIUtility.IconContent(ZStyle.ADD_BUTTON_ICON), ZStyle.HEADER_BUTTON_STYLE))
            {
                BuilderConfig.instance.packages.Add(PackageSeting.Create("New Package"));
            }
        }

        public override void OnDrawingHeaderRight(object userData)
        {
            if (userData is List<RulerData> list)
            {
                if (GUILayout.Button(EditorGUIUtility.IconContent(ZStyle.ADD_BUTTON_ICON), ZStyle.HEADER_BUTTON_STYLE))
                {
                    list.Add(new RulerData()
                    {
                        // use = true,
                        selector = new Selector(),
                        buildType = BuildType.Once
                    });
                }

                return;
            }

            if (GUILayout.Button(EditorGUIUtility.IconContent(ZStyle.DELETE_BUTTON_ICON), ZStyle.HEADER_BUTTON_STYLE))
            {
                BuilderConfig.instance.packages.Remove((PackageSeting)userData);
                ToolsWindow.Refresh();
            }

            if (GUILayout.Button(EditorGUIUtility.IconContent(ZStyle.PLAY_BUTTON_ICON), ZStyle.HEADER_BUTTON_STYLE))
            {
                BuildPackageCommand.Executer(userData);
                EditorUtility.DisplayDialog("打包完成", "资源打包成功", "OK");
                ToolsWindow.Refresh();
            }
        }

        public override void OnGUI()
        {
            for (int i = BuilderConfig.instance.packages.Count - 1; i >= 0; i--)
            {
                if (search.IsNullOrEmpty() is false && BuilderConfig.instance.packages[i].name.Contains(search) is false)
                {
                    continue;
                }

                PackageSeting package = BuilderConfig.instance.packages[i];
                package.isOn = OnBeginHeader(package.name, package.isOn, package);
                if (package.isOn)
                {
                    GUILayout.BeginVertical(ZStyle.BOX_BACKGROUND);
                    DrawingRuleInfo(BuilderConfig.instance.packages[i]);


                    GUILayout.EndVertical();
                }
            }
        }

        private void DrawingRuleInfo(PackageSeting package)
        {
            package.name = EditorGUILayout.TextField("规则名称", package.name);
            package.describe = EditorGUILayout.TextField("描述", package.describe);
            if (package.service == null || package.service.items == null || package.service.Count == 0)
            {
                package.service = new Selector();
            }

            package.service.Add(OSSConfig.instance.ossList.Select(x => x.title).ToArray());
            if (package.dependcies is null || package.dependcies is null || package.dependcies.Count == 0)
            {
                package.dependcies = new Selector();
            }

            package.dependcies.Add(BuilderConfig.instance.packages.Where(x => x.name != package.name).Select(x => x.name).ToArray());

            GUILayout.BeginHorizontal();
            GUILayout.Label("依赖包", GUILayout.Width(150));
            if (EditorGUILayout.DropdownButton(new GUIContent(package.dependcies.ToString()), FocusType.Passive))
            {
                package.dependcies.ShowContext(BuilderConfig.OnSave);
            }

            GUILayout.EndHorizontal();


            GUILayout.BeginHorizontal();
            GUILayout.Label("资源服", GUILayout.Width(150));
            if (EditorGUILayout.DropdownButton(new GUIContent(package.service.ToString()), FocusType.Passive))
            {
                package.service.ShowContext(BuilderConfig.OnSave);
            }

            GUILayout.EndHorizontal();


            if (package.items == null)
            {
                package.items = new List<RulerData>();
            }

            GUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button(EditorGUIUtility.IconContent(ZStyle.ADD_BUTTON_ICON), ZStyle.HEADER_BUTTON_STYLE))
            {
                package.items.Add(new RulerData()
                {
                    // use = true,
                    selector = new Selector(),
                    buildType = BuildType.Once
                });
            }

            GUILayout.EndHorizontal();
            // OnBeginHeader("Packages", true, package.items, false);
            // GUILayout.BeginVertical(ZStyle.BOX_BACKGROUND);
            for (int i = 0; i < package.items.Count; i++)
            {
                OnDrawingRuleItem(package.items[i], package);
                GUILayout.Space(5);
            }

            // GUILayout.EndVertical();
            GUILayout.EndVertical();
            if (Event.current.type == EventType.KeyDown && Event.current.control && Event.current.keyCode == KeyCode.S)
            {
                OnEnable();
                BuilderConfig.OnSave();
            }
        }

        private void OnDrawingRuleItem(RulerData rulerData, PackageSeting package)
        {
            GUILayout.BeginHorizontal(ZStyle.BOX_BACKGROUND);
            {
                // rulerData.use = EditorGUILayout.Toggle(rulerData.use, GUILayout.Width(20));
                rulerData.folder = EditorGUILayout.ObjectField(rulerData.folder, typeof(DefaultAsset), false);
                GUILayout.BeginHorizontal();
                {
                    rulerData.buildType = (BuildType)EditorGUILayout.EnumPopup("打包方式", rulerData.buildType, GUILayout.Width(300));
                    if (rulerData.folder != null && rulerData.buildType == BuildType.AssetType)
                    {
                        if (GUILayout.Button(rulerData.selector.ToString(), EditorStyles.popup, GUILayout.Width(200)))
                        {
                            GenericMenu menu = new GenericMenu();
                            menu.AddItem(new GUIContent("Noting"), rulerData.selector.isNone, () => { rulerData.selector.Clear(); });
                            menu.AddItem(new GUIContent("Everything"), rulerData.selector.isAll, () => { rulerData.selector.SelectAll(); });
                            foreach (var VARIABLE in rulerData.selector.items)
                            {
                                menu.AddItem(new GUIContent(VARIABLE.name), VARIABLE.isOn, () => { VARIABLE.isOn = !VARIABLE.isOn; });
                            }

                            menu.ShowAsContext();
                        }
                    }

                    GUILayout.EndHorizontal();
                }
                GUILayout.FlexibleSpace();
                if (GUILayout.Button(EditorGUIUtility.IconContent(ZStyle.DELETE_BUTTON_ICON), ZStyle.HEADER_BUTTON_STYLE))
                {
                    package.items.Remove(rulerData);
                    ToolsWindow.Refresh();
                }

                GUILayout.EndHorizontal();
            }
        }
    }
}