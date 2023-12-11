using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ZGame.Editor
{
    public partial class EditorManager : EditorWindow
    {
        private Vector2 menuRoll;
        private Vector2 pageRoll;
        private float leftWidth = 300;
        int start = 0;
        int end = 11;
        int cur = 0;
        string icon = "";
        public PageScene current { get; private set; }
        public string search { get; private set; }

        private void OnEnable()
        {
            current?.OnEnable();
        }

        private void OnDestroy()
        {
            _docker = null;
        }

        private void OnGUI()
        {
            if (current == null)
            {
                return;
            }

            EditorGUILayout.BeginHorizontal();
            DrawingMenuList();
            GUILayout.Space(5);

            DrawingPageScene();

            EditorGUILayout.EndHorizontal();
        }

        private void DrawingPageScene()
        {
            GUILayout.BeginVertical(ZStyle.GUI_STYLE_BOX_BACKGROUND, GUILayout.Width(position.width - 305), GUILayout.Height(position.height));
            GUILayout.BeginHorizontal();
            int size = EditorStyles.boldLabel.fontSize;
            EditorStyles.boldLabel.fontSize = 20;
            GUILayout.Label(current.name, EditorStyles.boldLabel);
            EditorStyles.boldLabel.fontSize = size;
            if (isWaiting)
            {
                GUILayout.Label(EditorGUIUtility.IconContent(icon, icon));
            }

            GUILayout.FlexibleSpace();
            EditorGUI.BeginDisabledGroup(isWaiting);
            search = EditorGUILayout.TextField(search, EditorStyles.toolbarSearchField, GUILayout.Width(300));
            GUILayout.EndHorizontal();

            this.BeginColor(ZStyle.inColor);
            GUILayout.Box("", ZStyle.GUI_STYLE_LINE, GUILayout.Height(1), GUILayout.Width(position.width - 305));
            this.EndColor();
            GUILayout.Space(5);

            pageRoll = GUILayout.BeginScrollView(pageRoll);
            current.position = new Rect(position.x + leftWidth + 5, position.y, position.width - 325, position.height);
            current.search = search;

            current.OnGUI();
            GUILayout.EndScrollView();
            EditorGUI.EndDisabledGroup();
            GUILayout.EndVertical();
        }

        private void DrawingMenuList()
        {
            GUILayout.BeginVertical(ZStyle.GUI_STYLE_BOX_BACKGROUND, GUILayout.MaxWidth(300), GUILayout.Height(position.height));
            menuRoll = GUILayout.BeginScrollView(menuRoll);
            foreach (var VARIABLE in sceneMaps)
            {
                if (OnDrwaingMeunItem(this.current.Equals(VARIABLE.scene), VARIABLE.scene.name, 0, VARIABLE.childs.Count > 0, ref VARIABLE.show))
                {
                    SwitchScene(VARIABLE.scene);
                }

                if (VARIABLE.childs.Count == 0 || VARIABLE.show is false)
                {
                    continue;
                }

                GUILayout.BeginHorizontal();
                GUILayout.Space(20);
                GUILayout.BeginVertical();
                foreach (var subScene in VARIABLE.childs)
                {
                    if (OnDrwaingMeunItem(this.current.Equals(subScene), subScene.name, 20, false, ref VARIABLE.show))
                    {
                        SwitchScene(subScene);
                    }
                }

                GUILayout.EndVertical();
                GUILayout.EndHorizontal();
            }

            GUILayout.EndScrollView();
            GUILayout.EndVertical();
        }

        public bool OnDrwaingMeunItem(bool isSelection, string name, float offset, bool isFoldout, ref bool show)
        {
            Rect contains = EditorGUILayout.BeginVertical();
            this.BeginColor(isSelection ? Color.cyan : GUI.color);
            GUILayout.BeginHorizontal();
            if (isFoldout)
            {
                GUILayout.BeginVertical(GUILayout.Width(20));
                GUILayout.Space(3);
                show = EditorGUILayout.Foldout(show, "");
                GUILayout.EndVertical();
                GUILayout.Space(-40);
            }

            GUILayout.Label(name, ZStyle.GUI_STYLE_TITLE_LABLE);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            this.EndColor();
            GUILayout.Space(5);
            this.BeginColor(current.Equals(this) ? ZStyle.inColor : ZStyle.outColor);
            GUILayout.BeginHorizontal();
            GUILayout.Space(-offset);
            GUILayout.Box("", ZStyle.GUI_STYLE_LINE, GUILayout.MaxWidth(leftWidth), GUILayout.Height(1));
            GUILayout.EndHorizontal();
            this.EndColor();
            bool result = false;
            if (Event.current.type == EventType.MouseDown && contains.Contains(Event.current.mousePosition) && Event.current.button == 0)
            {
                result = true;
            }

            GUILayout.Space(5);
            GUILayout.EndVertical();
            return result;
        }

        public void Waiting()
        {
            IEnumerator OnShow()
            {
                while (isWaiting)
                {
                    string temp = "WaitSpin";
                    if (cur < 10)
                    {
                        temp += "0" + cur;
                    }
                    else
                    {
                        temp += cur;
                    }

                    icon = temp;
                    Refresh();
                    yield return new EditorWaitForSeconds(0.1f);
                    cur++;
                    cur %= end;
                }
            }

            cur = 0;
            isWaiting = true;
            waiting = EditorCoroutineUtility.StartCoroutineOwnerless(OnShow());
        }

        public void CloseWaiting()
        {
            isWaiting = false;
            Refresh();
            instance.RemoveNotification();
            if (waiting == null)
            {
                return;
            }

            EditorCoroutineUtility.StopCoroutine(waiting);
        }
    }
}