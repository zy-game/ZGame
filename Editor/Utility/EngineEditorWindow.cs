using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using ZEngine.Editor.PlayerEditor;

namespace ZEngine.Editor
{
   
    public abstract class EngineEditorWindow : EditorWindow
    {
        private string search;
        private int selete;
        private MenuListItem[] items;
        private Vector2 listScroll;
        private Vector2 manifestScroll;
        private Color inColor = new Color(1f, 0.92f, 0.01f, .8f);
        private Color outColor = new Color(0, 0, 0, 0.2f);
        private float rightWidth;
        private float leftWidth;

        public const string GUI_STYLE_TITLE_LABLE = "LargeBoldLabel"; // new GUIStyle("LargeBoldLabel");
        public const string GUI_STYLE_BOX_BACKGROUND = "OL box NoExpand"; // new GUIStyle("OL box NoExpand");
        public const string GUI_STYLE_LINE = "WhiteBackground";
        public const string GUI_STYLE_ADD_BUTTON = "OL Plus";
        public const string GUI_STYLE_MINUS = "OL Minus";

        protected class MenuListItem
        {
            public int index;
            public string name;
            public string icon;
            public object data;
        }

        public class RightMeunItem
        {
            public string name;
            public Action callback;
        }

        protected virtual MenuListItem[] GetMenuList()
        {
            return Array.Empty<MenuListItem>();
        }

        protected virtual RightMeunItem[] GetRightButtonMeunList(object data)
        {
            return Array.Empty<RightMeunItem>();
        }

        protected virtual void OnDrawingToolbarMenu()
        {
        }

        protected abstract void Actived();
        protected abstract void CreateNewItem();
        protected abstract void DrawingItemDataView(object data, float width);
        protected abstract void SaveChanged();

        private void OnEnable()
        {
            this.minSize = new Vector2(1130, 640);
            Actived();
        }

        private void OnGUI()
        {
            leftWidth = Mathf.Min(position.width / 4, 300);
            rightWidth = position.width - leftWidth - 18;
            Toolbar();
            DrawingSceneEditor();
        }

        private void OnDisable()
        {
            SaveChanged();
        }

        void Toolbar()
        {
            GUILayout.BeginHorizontal(EditorStyles.toolbar);
            {
                if (GUILayout.Button("+", EditorStyles.toolbarButton))
                {
                    CreateNewItem();
                    SaveChanged();
                    this.Repaint();
                }

                OnDrawingToolbarMenu();
                GUILayout.FlexibleSpace();
                search = GUILayout.TextField(search, EditorStyles.toolbarSearchField, GUILayout.Width(leftWidth));
                GUILayout.EndHorizontal();
            }
        }


        void DrawingSceneEditor()
        {
            GUILayout.BeginHorizontal();
            {
                GUILayout.Space(5);
                GUILayout.BeginVertical();
                {
                    GUILayout.Space(5);
                    GUILayout.BeginVertical(GUI_STYLE_BOX_BACKGROUND, GUILayout.Width(leftWidth), GUILayout.Height(position.height - 30));
                    {
                        listScroll = GUILayout.BeginScrollView(listScroll);
                        {
                            DrawingSceneList();
                            GUILayout.EndScrollView();
                        }
                        GUILayout.EndVertical();
                    }
                    GUILayout.EndVertical();
                }


                GUILayout.BeginVertical();
                {
                    GUILayout.Space(5);
                    GUILayout.BeginVertical(GUI_STYLE_BOX_BACKGROUND, GUILayout.Width(rightWidth), GUILayout.Height(position.height - 30));
                    {
                        manifestScroll = GUILayout.BeginScrollView(manifestScroll, false, true);
                        {
                            DrawingSceneOptions();
                            GUILayout.EndScrollView();
                        }
                        GUILayout.EndVertical();
                    }
                    GUILayout.EndVertical();
                }
                GUILayout.EndHorizontal();
            }
        }

        private void DrawingSceneList()
        {
            items = GetMenuList();
            if (items is null || items.Length is 0)
            {
                return;
            }

            for (int i = 0; i < items.Length; i++)
            {
                if (search.IsNullOrEmpty() is false && items[i].name.Equals(search) is false)
                {
                    continue;
                }

                Rect contains = EditorGUILayout.BeginVertical();
                {
                    this.BeginColor(items[i].index == selete ? Color.cyan : GUI.color);
                    {
                        GUILayout.BeginHorizontal();
                        {
                            GUILayout.Label($"{items[i].name}", GUI_STYLE_TITLE_LABLE);
                            GUILayout.FlexibleSpace();
                            GUILayout.EndHorizontal();
                        }
                        this.EndColor();
                    }

                    GUILayout.Space(5);
                    this.BeginColor(items[i].index == selete ? inColor : outColor);
                    {
                        GUILayout.Box("", GUI_STYLE_LINE, GUILayout.Width(leftWidth), GUILayout.Height(1));
                        this.EndColor();
                    }
                    if (Event.current.type == EventType.MouseDown && contains.Contains(Event.current.mousePosition) && Event.current.button == 0)
                    {
                        selete = items[i].index;
                        this.Repaint();
                    }

                    if (Event.current.type == EventType.MouseDown && contains.Contains(Event.current.mousePosition) && Event.current.button == 1)
                    {
                        RightMeunItem[] list = GetRightButtonMeunList(items[i].data);
                        if (list is not null && list.Length > 0)
                        {
                            GenericMenu menu = new GenericMenu();
                            for (int j = 0; j < list.Length; j++)
                            {
                                GenericMenu.MenuFunction function = new GenericMenu.MenuFunction(list[j].callback);
                                menu.AddItem(new GUIContent(list[j].name), false, function);
                            }

                            menu.ShowAsContext();
                        }
                    }

                    GUILayout.EndVertical();
                    GUILayout.Space(5);
                }
            }
        }

        private void DrawingSceneOptions()
        {
            if (selete < 0)
            {
                return;
            }

            EditorGUI.BeginChangeCheck();
            var item = items.Where(x => x.index == selete).FirstOrDefault();
            if (item is null || item.data is null)
            {
                EditorGUI.EndChangeCheck();
                return;
            }

            DrawingItemDataView(item.data, rightWidth - 20);
            if (EditorGUI.EndChangeCheck())
            {
                SaveChanged();
            }
        }
    }
}