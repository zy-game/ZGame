using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using ZEngine.Editor.PlayerEditor;

namespace ZEngine.Editor
{
    public abstract class EngineCustomEditor : EditorWindow
    {
        private string search;
        private string SeleteName;
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
            public string name;
            public string icon;
            public object data;
        }

        private void OnEnable()
        {
            this.minSize = new Vector2(1130, 640);
            Actived();
        }

        public void OnGUI()
        {
            leftWidth = Mathf.Min(position.width / 4, 300);
            rightWidth = position.width - leftWidth - 18;
            Toolbar();
            DrawingSceneEditor();
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

        protected abstract MenuListItem[] GetMenuList();

        protected virtual void OnDrawingToolbarMenu()
        {
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

        protected abstract void Actived();
        protected abstract void CreateNewItem();
        protected abstract void DrawingItemDataView(object data, float width);
        protected abstract void SaveChanged();


        public class RightMeunItem
        {
            public string name;
            public Action callback;
        }

        protected virtual List<RightMeunItem> GetRightButtonMeunList(object data)
        {
            return default;
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
                    this.BeginColor(items[i].name == SeleteName ? Color.cyan : GUI.color);
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
                    this.BeginColor(items[i].name == SeleteName ? inColor : outColor);
                    {
                        GUILayout.Box("", GUI_STYLE_LINE, GUILayout.Width(leftWidth), GUILayout.Height(1));
                        this.EndColor();
                    }
                    if (Event.current.type == EventType.MouseDown && contains.Contains(Event.current.mousePosition) && Event.current.button == 0)
                    {
                        SeleteName = items[i].name;
                        this.Repaint();
                    }

                    if (Event.current.type == EventType.MouseDown && contains.Contains(Event.current.mousePosition) && Event.current.button == 1)
                    {
                        List<RightMeunItem> list = GetRightButtonMeunList(items[i].data);
                        if (list is not null && list.Count > 0)
                        {
                            GenericMenu menu = new GenericMenu();
                            for (int j = 0; j < list.Count; j++)
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

        void DrawingSceneOptions()
        {
            if (SeleteName.IsNullOrEmpty())
            {
                Debug.Log("??");
                return;
            }

            EditorGUI.BeginChangeCheck();
            var item = items.Where(x => x.name == SeleteName).FirstOrDefault();
            DrawingItemDataView(item.data, rightWidth - 20);
            if (EditorGUI.EndChangeCheck())
            {
                SaveChanged();
            }
        }
    }
}