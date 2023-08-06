using System;
using UnityEditor;
using UnityEngine;
using ZEngine.Resource;

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

        public void OnEnable()
        {
            options = new SerializedObject(ResourceOptions.instance);
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
            search = GUILayout.TextField(search, EditorStyles.toolbarSearchField, GUILayout.Width(300));
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Options", EditorStyles.toolbarButton))
            {
                isOptions = isOptions == Switch.Off ? Switch.On : Switch.Off;
            }

            if (GUILayout.Button("Build", EditorStyles.toolbarButton))
            {
            }

            GUILayout.EndHorizontal();
        }

        void DrawingOptions()
        {
            EditorGUI.BeginChangeCheck();
            GUILayout.BeginVertical("Module Options", EditorStyles.helpBox);
            GUILayout.Space(10);
            EditorGUILayout.PropertyField(options.FindProperty("modules"), true);
            GUILayout.EndVertical();

            if (EditorGUI.EndChangeCheck())
            {
                options.ApplyModifiedProperties();
                ResourceOptions.instance.Saved();
            }
        }

        private Vector2 listScroll = Vector2.zero;
        private Vector2 manifestScroll = Vector2.zero;

        void DrawingResourceModule()
        {
            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.Width(300), GUILayout.Height(position.height - 25));
            listScroll = GUILayout.BeginScrollView(listScroll);
            DrawingMoudleList();
            GUILayout.EndScrollView();
            GUILayout.EndVertical();

            GUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.Width(position.width - 310), GUILayout.Height(position.height - 25));
            manifestScroll = GUILayout.BeginScrollView(manifestScroll);
            DrawingModuleManifest();
            GUILayout.EndScrollView();
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
        }

        private ResourceModuleManifest selection;

        private void DrawingMoudleList()
        {
            if (ResourceOptions.instance.modules is null || ResourceOptions.instance.modules.Count is 0)
            {
                return;
            }


            for (int i = 0; i < ResourceOptions.instance.modules.Count; i++)
            {
                ResourceModuleManifest moduleManifest = ResourceOptions.instance.modules[i];
                if (search.IsNullOrEmpty() is false)
                {
                    if (moduleManifest.Search(search) is false)
                    {
                        continue;
                    }
                }

                GUILayout.BeginVertical();
                GUILayout.BeginHorizontal();
                if (GUILayout.Button(moduleManifest.title, "LargeBoldLabel", GUILayout.Width(260)))
                {
                    selection = moduleManifest;
                    this.Repaint();
                }

                GUILayout.FlexibleSpace();
                GUILayout.BeginVertical();
                GUILayout.Space(5);
                GUILayout.Label("", "PaneOptions");
                GUILayout.EndVertical();
                GUILayout.EndHorizontal();
                GUILayout.Space(5);
                Color color = GUI.color;
                GUI.color = moduleManifest == selection ? Color.white : new Color(0, 0, 0, .2f);
                GUILayout.Box("", "WhiteBackground", GUILayout.Width(290), GUILayout.Height(1));
                GUI.color = color;
                GUILayout.EndVertical();
                GUILayout.Space(5);
            }
        }

        private void DrawingModuleManifest()
        {
            if (selection is null)
            {
                return;
            }
        }
    }
}