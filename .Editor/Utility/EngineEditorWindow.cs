using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using ZEngine.Editor.MapEditor;
using ZEngine.Editor.PlayerEditor;

namespace ZEngine.Editor
{
    public abstract class EngineEditorWindow : EditorWindow
    {
        private string search;
        private object selete;
        private Vector2 listScroll;
        private Vector2 manifestScroll;
        private float rightWidth;
        private float leftWidth;

        protected class MenuListItem
        {
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

        protected virtual void Actived()
        {
        }

        protected virtual void SaveChanged()
        {
        }

        protected virtual void DrawingItemDataView(object data, float width)
        {
            DrawingProperties(data);
        }

        public void DrawingProperties(object obj)
        {
        }

        private void OnEnable()
        {
            this.minSize = new Vector2(1130, 640);
            Actived();
        }

        private void OnDisable()
        {
            SaveChanged();
        }

        private void OnGUI()
        {
            leftWidth = Mathf.Min(position.width / 4, 300);
            rightWidth = position.width - leftWidth - 18;
            GUILayout.BeginHorizontal(EditorStyles.toolbar);
            {
                OnDrawingToolbarMenu();
                GUILayout.FlexibleSpace();
                search = GUILayout.TextField(search, EditorStyles.toolbarSearchField, GUILayout.Width(leftWidth));
                GUILayout.EndHorizontal();
            }
            GUILayout.BeginHorizontal();
            {
                GUILayout.Space(5);
                GUILayout.BeginVertical();
                {
                    GUILayout.Space(5);
                    GUILayout.BeginVertical(CustomWindowStyle.GUI_STYLE_BOX_BACKGROUND, GUILayout.Width(leftWidth), GUILayout.Height(position.height - 30));
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
                    GUILayout.BeginVertical(CustomWindowStyle.GUI_STYLE_BOX_BACKGROUND, GUILayout.Width(rightWidth), GUILayout.Height(position.height - 30));
                    {
                        manifestScroll = GUILayout.BeginScrollView(manifestScroll, false, true);
                        {
                            if (selete is not null)
                            {
                                EditorGUI.BeginChangeCheck();
                                DrawingItemDataView(selete, rightWidth - 20);
                                if (EditorGUI.EndChangeCheck())
                                {
                                    SaveChanged();
                                }
                            }

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
            var items = GetMenuList();
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
                    this.BeginColor(items[i].data.Equals(selete) ? Color.cyan : GUI.color, () =>
                    {
                        GUILayout.BeginHorizontal();
                        {
                            GUILayout.Label($"{items[i].name}", CustomWindowStyle.GUI_STYLE_TITLE_LABLE);
                            GUILayout.FlexibleSpace();
                            GUILayout.EndHorizontal();
                        }
                    });


                    GUILayout.Space(5);
                    Color color = items[i].data.Equals(selete) ? CustomWindowStyle.inColor : CustomWindowStyle.outColor;
                    this.BeginColor(color, () => { GUILayout.Box("", CustomWindowStyle.GUI_STYLE_LINE, GUILayout.Width(leftWidth), GUILayout.Height(1)); });
                    this.OnMouseLeftButtonDown(contains, () =>
                    {
                        ZGame.Console.Log("left");
                        selete = items[i].data;
                        this.Repaint();
                    });

                    this.OnMouseRightButtomDown(contains, () =>
                    {
                        ZGame.Console.Log("right");
                        RightMeunItem[] list = GetRightButtonMeunList(items[i].data);
                        if (list is not null && list.Length > 0)
                        {
                            GenericMenu menu = new GenericMenu();
                            for (int j = 0; j < list.Length; j++)
                            {
                                menu.AddItem(new GUIContent(list[j].name), false, new GenericMenu.MenuFunction(list[j].callback));
                            }

                            menu.ShowAsContext();
                        }
                    });

                    GUILayout.EndVertical();
                    GUILayout.Space(5);
                }
            }
        }
    }
}