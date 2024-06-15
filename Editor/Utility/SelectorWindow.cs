using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ZGame.Editor
{
    public class SelectorItem
    {
        public bool isOn;
        public Texture icon;
        public string name;
        public object userData;
        public bool isClick;

        public SelectorItem(string name, object userData, bool isOn = false)
        {
            this.name = name;
            this.isOn = isOn;
            this.userData = userData;
        }

        public SelectorItem(Texture icon, string name, object userData, bool isOn = false) : this(name, userData, isOn)
        {
            this.icon = icon;
        }
    }

    public class SelectorWindow : PopupWindowContent
    {
        private Rect _rect;
        private bool isMuilt;
        private Vector2 scroll;
        private string search;
        private List<SelectorItem> itemList;
        private Action<IEnumerable<SelectorItem>> callback;

        public override Vector2 GetWindowSize()
        {
            return new Vector2(250, Math.Clamp(itemList.Count() * 20 + 60, 50, 400));
        }


        void OnComplete()
        {
            if (itemList.Exists(x => x.isOn) is false)
            {
                return;
            }

            this.callback?.Invoke(itemList.Where(x => x.isOn));
        }

        public override void OnGUI(Rect rect)
        {
            GUILayout.BeginHorizontal(ZStyle.BOX_BACKGROUND);
            EditorGUILayout.LabelField("Selection", EditorStyles.boldLabel);
            if (isMuilt)
            {
                GUILayout.FlexibleSpace();
                if (GUILayout.Button(EditorGUIUtility.IconContent("TestPassed"), GUIStyle.none, GUILayout.Width(25)))
                {
                    OnComplete();
                    EditorWindow.focusedWindow.Close();
                }
            }

            EditorGUILayout.EndHorizontal();
            GUILayoutTools.DrawLine(ZStyle.inColor);
            GUILayout.Space(2);
            search = EditorGUILayout.TextField(search);
            scroll = EditorGUILayout.BeginScrollView(scroll);
            foreach (var VARIABLE in itemList)
            {
                if (search.IsNullOrEmpty() is false && VARIABLE.name.StartsWith(search) is false)
                {
                    continue;
                }

                GUILayoutTools.DrawBackground(VARIABLE.isClick ? ZStyle.selectColor : ZStyle.darkColor, 20);
                OnDrawItem(VARIABLE);
            }

            EditorGUILayout.EndScrollView();
        }

        private void OnDrawItem(SelectorItem item)
        {
            Rect click = EditorGUILayout.BeginHorizontal();
            if (isMuilt)
            {
                item.isOn = EditorGUILayout.Toggle(item.isOn, GUILayout.Width(20));
            }

            if (item.icon != null)
            {
                GUILayout.Label(item.icon);
            }

            EditorGUILayout.LabelField(item.name);
            if (Event.current.type == EventType.MouseDown && click.Contains(Event.current.mousePosition))
            {
                if (Event.current.clickCount == 1)
                {
                    itemList.ForEach(x => x.isClick = false);
                    item.isClick = true;
                }

                if (Event.current.clickCount == 2)
                {
                    item.isOn = true;
                    Event.current.Use();
                    if (isMuilt is false)
                    {
                        OnComplete();
                        EditorWindow.focusedWindow.Close();
                    }
                }

                EditorWindow.focusedWindow.Repaint();
            }

            EditorGUILayout.EndHorizontal();
            GUILayout.Space(-2);
            GUILayoutTools.DrawLine(ZStyle.outColor);
        }


        public static SelectorWindow ShowMulit(IEnumerable<SelectorItem> enumerable, Action<IEnumerable<SelectorItem>> comfirm)
        {
            SelectorWindow selectorWindow = new SelectorWindow();
            selectorWindow.itemList = enumerable.ToList();
            selectorWindow.callback = comfirm;
            selectorWindow.isMuilt = true;
            PopupWindow.Show(new Rect(Event.current.mousePosition, Vector2.zero), selectorWindow);
            return selectorWindow;
        }

        public static SelectorWindow Show(IEnumerable<SelectorItem> enumerable, Action<IEnumerable<SelectorItem>> comfirm)
        {
            SelectorWindow selectorWindow = new SelectorWindow();
            selectorWindow.itemList = enumerable.ToList();
            selectorWindow.callback = comfirm;
            selectorWindow.isMuilt = false;
            PopupWindow.Show(new Rect(Event.current.mousePosition, Vector2.zero), selectorWindow);
            return selectorWindow;
        }
    }
}