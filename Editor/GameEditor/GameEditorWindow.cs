using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ZGame.Editor
{
    public class GameEditorWindow : EditorWindow
    {
        [MenuItem("Tools/ZGame Studio Editor %g")]
        static void Open()
        {
            GetWindow<GameEditorWindow>(false, "ZGame Studio", true).Show();
        }

        class MenuTreeItem
        {
            public bool selected;
            public EditorSceneWindow main;
            public List<EditorSceneWindow> children = new();
        }

        private Vector2 menuTreeScroll = Vector2.zero;
        private Vector2 sceneWindowScroll = Vector2.zero;
        private EditorSceneWindow currentSceneWindow;
        private List<MenuTreeItem> menuTreeList = new();

        private void OnDisable()
        {
            foreach (var item in menuTreeList)
            {
                item.main?.OnDisable();
                foreach (var subItem in item.children)
                {
                    subItem.OnDisable();
                }
            }
        }

        private void OnDestroy()
        {
            foreach (var item in menuTreeList)
            {
                item.main?.OnSave();
                foreach (var subItem in item.children)
                {
                    subItem.OnSave();
                }
            }
        }

        [UnityEditor.Callbacks.DidReloadScripts]
        static void AllScriptsReloaded()
        {
            if (EditorWindow.HasOpenInstances<GameEditorWindow>() is false)
            {
                return;
            }

            if (EditorPrefs.HasKey("GameEditorWindow_CurrentSceneWindow") is false)
            {
                return;
            }

            GameEditorWindow window = EditorWindow.GetWindow<GameEditorWindow>();
            string currentSceneWindowName = EditorPrefs.GetString("GameEditorWindow_CurrentSceneWindow");
            foreach (var VARIABLE in window.menuTreeList)
            {
                if (VARIABLE.main.name == currentSceneWindowName)
                {
                    VARIABLE.selected = true;
                    window.currentSceneWindow = VARIABLE.main;
                    continue;
                }

                foreach (var child in VARIABLE.children)
                {
                    if (child.name == currentSceneWindowName)
                    {
                        VARIABLE.selected = true;
                        window.currentSceneWindow = child;
                    }
                }
            }

            window.currentSceneWindow?.Awake();
            window.currentSceneWindow?.OnEnable();
            window.currentSceneWindow?.OnScriptCompiled();
        }

        public void OnEnable()
        {
            menuTreeList.Clear();
            currentSceneWindow = null;
            var homeList = AppDomain.CurrentDomain.GetAllSubClasses<HomeEditorSceneWindow>();
            foreach (var item in homeList)
            {
                var menuItem = new MenuTreeItem();
                menuItem.main = (HomeEditorSceneWindow)Activator.CreateInstance(item);
                menuItem.main.Awake();
                this.menuTreeList.Add(menuItem);
            }

            var subList = AppDomain.CurrentDomain.GetAllSubClasses<SubEditorSceneWindow>();
            foreach (var VARIABLE in subList)
            {
                SubEditorSceneWindow subEditorSceneWindow = default;
                if (VARIABLE.BaseType.GenericTypeArguments is null || VARIABLE.BaseType.GenericTypeArguments.Length == 0)
                {
                    subEditorSceneWindow = (SubEditorSceneWindow)Activator.CreateInstance(VARIABLE);
                    subEditorSceneWindow.Awake();
                    menuTreeList.Find(x => x.GetType() == subEditorSceneWindow.owner).children.Add(subEditorSceneWindow);
                    continue;
                }

                string[] guildList = AssetDatabase.FindAssets($"t:{VARIABLE.BaseType.GenericTypeArguments.FirstOrDefault()}");
                foreach (var guid in guildList)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    Object cfg = AssetDatabase.LoadAssetAtPath(path, VARIABLE.BaseType.GenericTypeArguments.FirstOrDefault());
                    subEditorSceneWindow = (SubEditorSceneWindow)Activator.CreateInstance(VARIABLE, new object[] { cfg });
                    subEditorSceneWindow.Awake();
                    menuTreeList.Find(x => x.main.GetType() == subEditorSceneWindow.owner).children.Add(subEditorSceneWindow);
                }
            }
        }


        private void OnGUI()
        {
            if (UnityEngine.Event.current.control && UnityEngine.Event.current.keyCode == (KeyCode.S))
            {
                currentSceneWindow.OnSave();
                UnityEngine.Event.current.Use();
            }

            EditorGUILayout.BeginHorizontal();
            menuTreeScroll = EditorGUILayout.BeginScrollView(menuTreeScroll,
                false,
                false,
                GUIStyle.none,
                GUIStyle.none,
                ZStyle.BOX_BACKGROUND,
                GUILayout.Width(position.width * 0.25f),
                GUILayout.Height(position.height - 2));

            for (int j = 0; j < menuTreeList.Count; j++)
            {
                var item = menuTreeList[j];
                item.selected = OnDrawSceneMenuitem(item.main, item.selected, false);
                if (item.selected)
                {
                    for (int i = 0; i < item.children.Count; i++)
                    {
                        OnDrawSceneMenuitem(item.children[i], false, true);
                    }
                }
            }

            EditorGUILayout.EndScrollView();

            if (currentSceneWindow is not null)
            {
                EditorGUILayout.BeginVertical(ZStyle.BOX_BACKGROUND);

                Rect rect = EditorGUILayout.BeginHorizontal();
                if (currentSceneWindow is SubEditorSceneWindow subEditorSceneWindow)
                {
                    string text = GUILayoutTools.DrawEditLabel(currentSceneWindow.name, EditorStyles.boldLabel);
                    if (text != currentSceneWindow.name)
                    {
                        subEditorSceneWindow.SetTitleName(text);
                        this.Repaint();
                    }
                }
                else
                {
                    EditorGUILayout.LabelField(currentSceneWindow.name, EditorStyles.boldLabel);
                }

                GUILayout.FlexibleSpace();
                currentSceneWindow.OnDrawToolbar();
                EditorGUILayout.EndHorizontal();
                GUILayout.Space(3);
                GUILayoutTools.DrawLine(ZStyle.inColor);
                sceneWindowScroll = EditorGUILayout.BeginScrollView(sceneWindowScroll,
                    false,
                    false,
                    GUIStyle.none,
                    GUIStyle.none,
                    GUIStyle.none,
                    GUILayout.Width((position.width * 0.75f)),
                    GUILayout.Height(position.height - 34));
                GUILayout.Space(10);
                currentSceneWindow?.OnGUI();
                EditorGUILayout.EndScrollView();
                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.EndHorizontal();
        }

        private bool OnDrawSceneMenuitem(EditorSceneWindow sceneWindow, bool isSelect, bool isChild = false)
        {
            GUILayoutTools.DrawBackground(currentSceneWindow == sceneWindow ? ZStyle.selectColor : ZStyle.darkColor, 24, 2);
            Rect rect = EditorGUILayout.BeginHorizontal();
            if (!isChild)
            {
                GUILayout.BeginVertical();
                GUILayout.Space(3);
                isSelect = EditorGUILayout.Toggle(isSelect, EditorStyles.foldout, GUILayout.Width(15));
                GUILayout.EndVertical();
            }

            GUILayout.Space(isChild ? 20 : 0);
            EditorGUILayout.LabelField(new GUIContent(sceneWindow.name), isChild ? EditorStyles.label : EditorStyles.boldLabel);


            GUILayout.FlexibleSpace();
            // if (sceneWindow is SubEditorSceneWindow subEditorSceneWindow)
            // {
            //     GUILayout.BeginVertical();
            //     GUILayout.Space(5);
            //     if (GUILayout.Button(EditorGUIUtility.IconContent(ZStyle.DELETE_BUTTON_ICON), GUIStyle.none, GUILayout.Height(18)))
            //     {
            //         subEditorSceneWindow.OnDelete();
            //         this.OnEnable();
            //         this.Repaint();
            //     }
            //
            //     GUILayout.EndVertical();
            // }
            // else
            // {
            //     GUILayout.BeginVertical();
            //     GUILayout.Space(5);
            //     if (GUILayout.Button(EditorGUIUtility.IconContent(ZStyle.ADD_BUTTON_ICON), GUIStyle.none, GUILayout.Height(18)))
            //     {
            //         sceneWindow.OnCreate();
            //         this.OnEnable();
            //         this.Repaint();
            //     }
            //
            //     GUILayout.EndVertical();
            // }


            if (Event.current.type == EventType.MouseDown && rect.Contains(Event.current.mousePosition))
            {
                Event.current.Use();
                Switch(sceneWindow);
            }

            EditorGUILayout.EndHorizontal();
            GUILayoutTools.DrawLine(currentSceneWindow == sceneWindow ? ZStyle.inColor : ZStyle.outColor, 2);
            return isSelect;
        }

        public void Switch(EditorSceneWindow sceneWindow)
        {
            currentSceneWindow?.OnDisable();
            currentSceneWindow = sceneWindow;
            currentSceneWindow.OnEnable();
            EditorPrefs.SetString("GameEditorWindow_CurrentSceneWindow", sceneWindow.name);
            this.Repaint();
        }
    }
}