using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using ZEngine.Resource;
using Object = UnityEngine.Object;

namespace ZEngine.Editor.ResourceBuilder
{
    public class GUIStyleViewer : EditorWindow
    {
        private Vector2 scrollVector2 = Vector2.zero;
        private string search = "";

        [MenuItem("UFramework/GUIStyle查看器")]
        public static void InitWindow()
        {
            EditorWindow.GetWindow(typeof(GUIStyleViewer));
        }

        void OnGUI()
        {
            GUILayout.BeginHorizontal("HelpBox");
            GUILayout.Space(30);
            search = EditorGUILayout.TextField("", search, "SearchTextField", GUILayout.MaxWidth(position.x / 3));
            GUILayout.Label("", "SearchCancelButtonEmpty");
            GUILayout.EndHorizontal();
            scrollVector2 = GUILayout.BeginScrollView(scrollVector2);
            foreach (GUIStyle style in GUI.skin.customStyles)
            {
                if (style.name.ToLower().Contains(search.ToLower()))
                {
                    DrawStyleItem(style);
                }
            }

            GUILayout.EndScrollView();
        }

        void DrawStyleItem(GUIStyle style)
        {
            GUILayout.BeginHorizontal("box");
            GUILayout.Space(40);
            EditorGUILayout.SelectableLabel(style.name);
            GUILayout.FlexibleSpace();
            EditorGUILayout.SelectableLabel(style.name, style);
            GUILayout.Space(40);
            EditorGUILayout.SelectableLabel("", style, GUILayout.Height(40), GUILayout.Width(40));
            GUILayout.Space(50);
            if (GUILayout.Button("复制到剪贴板"))
            {
                TextEditor textEditor = new TextEditor();
                textEditor.text = style.name;
                textEditor.OnFocus();
                textEditor.Copy();
            }

            GUILayout.EndHorizontal();
            GUILayout.Space(10);
        }
    }

    public class GameResourceBuilder : EditorWindow
    {
        [MenuItem("Game/Build Package")]
        public static void Open()
        {
            GetWindow<GameResourceBuilder>(false, "Package Builder", true);
        }

        private string search;
        private SerializedObject options;
        private Switch isOptions = Switch.Off;
        private ResourceModuleManifest selection;
        private Vector2 listScroll = Vector2.zero;
        private Vector2 optionsScroll = Vector2.zero;
        private Vector2 manifestScroll = Vector2.zero;

        public void OnEnable()
        {
            options = new SerializedObject(ResourceModuleOptions.instance);
            if (ResourceModuleOptions.instance.modules is null || ResourceModuleOptions.instance.modules.Count is 0)
            {
                return;
            }

            foreach (var module in ResourceModuleOptions.instance.modules)
            {
                if (module.folder == null)
                {
                    continue;
                }

                foreach (var bundle in module.bundles)
                {
                    if (bundle.folder == null)
                    {
                        continue;
                    }

                    string path = AssetDatabase.GetAssetPath(bundle.folder);
                    string[] files = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories);
                    bundle.files = new List<Object>();
                    foreach (var VARIABLE in files)
                    {
                        if (VARIABLE.EndsWith(".meta"))
                        {
                            continue;
                        }

                        bundle.files.Add(AssetDatabase.LoadAssetAtPath<Object>(VARIABLE));
                    }
                }
            }
        }

        public void OnGUI()
        {
            Toolbar();
            switch (isOptions)
            {
                case Switch.On:
                    DrawingOptions();
                    break;
                case Switch.Off:
                    DrawingResourceModule();
                    break;
            }
        }

        void Toolbar()
        {
            GUILayout.BeginHorizontal(EditorStyles.toolbar);
            {
                GUILayout.FlexibleSpace();
                search = GUILayout.TextField(search, EditorStyles.toolbarSearchField, GUILayout.Width(300));
                if (GUILayout.Button("Options", EditorStyles.toolbarButton))
                {
                    isOptions = isOptions == Switch.Off ? Switch.On : Switch.Off;
                }

                if (GUILayout.Button("Build", EditorStyles.toolbarButton))
                {
                    if (ResourceModuleOptions.instance.modules is null || ResourceModuleOptions.instance.modules.Count is 0)
                    {
                        return;
                    }

                    List<ResourceBundleManifest> manifests = new List<ResourceBundleManifest>();
                    foreach (var VARIABLE in ResourceModuleOptions.instance.modules)
                    {
                        manifests.AddRange(VARIABLE.bundles);
                    }

                    OnBuild(manifests.ToArray());
                }

                GUILayout.EndHorizontal();
            }
        }

        void DrawingOptions()
        {
            optionsScroll = GUILayout.BeginScrollView(optionsScroll);
            {
                EditorGUI.BeginChangeCheck();
                {
                    GUILayout.BeginVertical("Options", EditorStyles.helpBox);
                    {
                        GUILayout.Space(10);
                        EditorGUILayout.PropertyField(options.FindProperty("options"), true);
                        GUILayout.EndVertical();
                    }
                    GUILayout.BeginVertical("Module Options", EditorStyles.helpBox);
                    {
                        GUILayout.Space(10);
                        EditorGUILayout.PropertyField(options.FindProperty("modules"), true);
                        GUILayout.EndVertical();
                    }
                    if (EditorGUI.EndChangeCheck())
                    {
                        options.ApplyModifiedProperties();
                        ResourceModuleOptions.instance.Saved();
                    }
                }
                GUILayout.EndScrollView();
            }
        }

        void DrawingResourceModule()
        {
            GUILayout.BeginHorizontal();
            {
                GUILayout.Space(5);
                GUILayout.BeginVertical();
                {
                    GUILayout.Space(5);
                    GUILayout.BeginVertical("OL box NoExpand", GUILayout.Width(300), GUILayout.Height(position.height - 30));
                    {
                        listScroll = GUILayout.BeginScrollView(listScroll);
                        {
                            DrawingMoudleList();
                            GUILayout.EndScrollView();
                        }
                        GUILayout.EndVertical();
                    }
                    GUILayout.EndVertical();
                }


                GUILayout.BeginVertical();
                {
                    GUILayout.Space(5);
                    GUILayout.BeginVertical("OL box NoExpand", GUILayout.Width(position.width - 310), GUILayout.Height(position.height - 30));
                    {
                        manifestScroll = GUILayout.BeginScrollView(manifestScroll, false, true);
                        {
                            DrawingModuleManifest();
                            GUILayout.EndScrollView();
                        }
                        GUILayout.EndVertical();
                    }
                    GUILayout.EndVertical();
                }
                GUILayout.EndHorizontal();
            }
        }


        private void DrawingMoudleList()
        {
            if (ResourceModuleOptions.instance.modules is null || ResourceModuleOptions.instance.modules.Count is 0)
            {
                return;
            }


            for (int i = 0; i < ResourceModuleOptions.instance.modules.Count; i++)
            {
                ResourceModuleManifest moduleManifest = ResourceModuleOptions.instance.modules[i];
                if (search.IsNullOrEmpty() is false && moduleManifest.Search(search) is false)
                {
                    continue;
                }

                GUILayout.BeginVertical();
                {
                    Color back = GUI.color;
                    GUI.color = moduleManifest == selection ? Color.cyan : back;
                    GUILayout.BeginHorizontal();
                    {
                        if (GUILayout.Button(moduleManifest.title, "LargeBoldLabel", GUILayout.Width(260)))
                        {
                            selection = moduleManifest;
                            this.Repaint();
                        }

                        GUILayout.FlexibleSpace();
                        GUILayout.BeginVertical();
                        {
                            GUILayout.Space(5);
                            if (GUILayout.Button("", "PaneOptions"))
                            {
                                GenericMenu menu = new GenericMenu();
                                menu.AddItem(new GUIContent("Build"), false, () => { OnBuild(moduleManifest.bundles.ToArray()); });
                                menu.AddItem(new GUIContent("Delete"), false, () =>
                                {
                                    ResourceModuleOptions.instance.modules.Remove(moduleManifest);
                                    ResourceModuleOptions.instance.Saved();
                                    this.Repaint();
                                });
                                menu.ShowAsContext();
                            }

                            GUILayout.EndVertical();
                        }

                        GUILayout.EndHorizontal();
                    }
                    GUI.color = back;
                    GUILayout.Space(5);
                    Color color = GUI.color;
                    GUI.color = moduleManifest == selection ? new Color(1f, 0.92156863f, 0.015686275f, .5f) : new Color(0, 0, 0, .2f);
                    GUILayout.Box("", "WhiteBackground", GUILayout.Width(300), GUILayout.Height(1));
                    GUI.color = color;
                }

                GUILayout.EndVertical();
                GUILayout.Space(5);
            }
        }

        private void DrawingModuleManifest()
        {
            if (selection == null)
            {
                return;
            }

            if (selection.folder == null || selection.bundles == null || selection.bundles.Count == 0)
            {
                return;
            }

            for (int i = 0; i < selection.bundles.Count; i++)
            {
                ResourceBundleManifest manifest = selection.bundles[i];

                if (manifest.folder == null)
                {
                    continue;
                }

                GUILayout.BeginVertical();
                {
                    GUILayout.BeginHorizontal();
                    {
                        GUILayout.BeginVertical();
                        {
                            GUILayout.Space(5);
                            manifest.isOn = GUILayout.Toggle(manifest.isOn, "");
                            GUILayout.EndVertical();
                            string name = (manifest.name.IsNullOrEmpty() ? $"Empty" : manifest.name) + $"({AssetDatabase.GetAssetPath(manifest.folder)}) ver:{manifest.version.ToString()}";
                            if (GUILayout.Button(name, "LargeBoldLabel", GUILayout.Width(position.width - 370)))
                            {
                                manifest.foldout = !manifest.foldout;
                                this.Repaint();
                            }

                            GUILayout.FlexibleSpace();
                            GUILayout.BeginVertical();
                            {
                                GUILayout.Space(5);
                                if (GUILayout.Button("", "PaneOptions"))
                                {
                                    GenericMenu menu = new GenericMenu();
                                    menu.AddItem(new GUIContent("Build"), false, () => { OnBuild(manifest); });
                                    menu.AddItem(new GUIContent("Delete"), false, () =>
                                    {
                                        selection.bundles.Remove(manifest);
                                        ResourceModuleOptions.instance.Saved();
                                    });
                                    menu.ShowAsContext();
                                }

                                GUILayout.EndVertical();
                            }
                        }
                        GUILayout.EndHorizontal();
                    }
                    GUILayout.Space(5);
                    Color color = GUI.color;
                    GUI.color = new Color(0, 0, 0, .2f);
                    GUILayout.Box("", "WhiteBackground", GUILayout.Height(1));
                    GUI.color = color;
                    GUILayout.EndVertical();
                }

                if (manifest.foldout && manifest.files is not null && manifest.files.Count > 0)
                {
                    foreach (var VARIABLE in manifest.files)
                    {
                        if (search.IsNullOrEmpty() is false && VARIABLE.name.Contains(search) is false)
                        {
                            continue;
                        }

                        GUILayout.BeginHorizontal();
                        {
                            GUILayout.Space(23);
                            EditorGUILayout.ObjectField(VARIABLE, typeof(Object));
                            GUILayout.EndHorizontal();
                        }
                    }
                }

                GUILayout.Space(5);
            }
        }

        private void OnBuild(params ResourceBundleManifest[] manifests)
        {
            AssetBundleBuild[] builds = new AssetBundleBuild[manifests.Length];
            for (int i = 0; i < manifests.Length; i++)
            {
                if (manifests[i].folder == null || manifests[i].files is null || manifests[i].files.Count is 0)
                {
                    continue;
                }

                builds[i] = new AssetBundleBuild()
                {
                    assetBundleName = manifests[i].name.IsNullOrEmpty() ? manifests[i].folder.name : manifests[i].name,
                    assetNames = manifests[i].files.Select(x => AssetDatabase.GetAssetPath(x)).ToArray()
                };
            }

            string output = Application.dataPath + "/../output/assets/" + Engine.Custom.GetPlatfrom();
            if (Directory.Exists(output) is false)
            {
                Directory.CreateDirectory(output);
            }

            try
            {
                AssetBundleManifest bundleManifest = BuildPipeline.BuildAssetBundles(output, builds, BuildAssetBundleOptions.None, EditorUserBuildSettings.activeBuildTarget);
                List<RuntimeModuleManifest> runtimeModuleManifests = new List<RuntimeModuleManifest>();
                if (File.Exists(output + "/module.ini"))
                {
                    runtimeModuleManifests = Engine.Json.Parse<List<RuntimeModuleManifest>>(File.ReadAllText(output + "/module.ini"));
                }

                for (int i = 0; i < manifests.Length; i++)
                {
                    manifests[i].version.Up();
                    ResourceModuleManifest resourceModuleManifest = ResourceModuleOptions.instance.modules.Find(x => x.bundles.Contains(manifests[i]));
                    RuntimeModuleManifest runtimeModuleManifest = runtimeModuleManifests.Find(x => x.name == resourceModuleManifest.title);
                    if (runtimeModuleManifest is null)
                    {
                        runtimeModuleManifest = new RuntimeModuleManifest();
                        runtimeModuleManifest.name = resourceModuleManifest.title;
                        runtimeModuleManifest.version = new VersionOptions();
                        runtimeModuleManifest.bundleList = new List<RuntimeBundleManifest>();
                    }
                    
                }

                ResourceModuleOptions.instance.Saved();
                this.Repaint();
            }
            catch (Exception e)
            {
                Engine.Console.Error(e);
            }
        }
    }
}