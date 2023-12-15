using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ZGame.Editor.Baker
{
    public class ObjectSelectionWindow : PopupWindowContent
    {
        private bool[] isSelect;
        private IList _source;
        private IList _selection;
        private string search;
        private Vector2 scroll;
        private Action _action;

        public static void Show<T>(Rect rect, List<T> selection, List<T> source, Action closeCallback = null)
        {
            ObjectSelectionWindow selectionWindow = new ObjectSelectionWindow();
            selectionWindow._source = source;
            selectionWindow._selection = selection;
            selectionWindow.isSelect = new bool[source.Count];
            selectionWindow._action = closeCallback;
            PopupWindow.Show(rect, selectionWindow);
        }

        public override Vector2 GetWindowSize()
        {
            return new Vector2(400, 600);
        }

        public override void OnOpen()
        {
            for (int i = 0; i < _source.Count; i++)
            {
                if (_selection.Contains(_source[i]))
                {
                    isSelect[i] = true;
                }
            }
        }

        public override void OnClose()
        {
            _selection.Clear();
            for (int i = 0; i < isSelect.Length; i++)
            {
                if (isSelect[i])
                {
                    _selection.Add(_source[i]);
                }
            }

            _action?.Invoke();
        }

        public override void OnGUI(Rect rect)
        {
            if (_source == null || _source.Count == 0)
            {
                return;
            }

            GUILayout.BeginHorizontal(ZStyle.GUI_STYLE_BOX_BACKGROUND);
            EditorGUILayout.LabelField("Selection", EditorStyles.boldLabel, GUILayout.ExpandWidth(true));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            search = GUILayout.TextField(search, GUILayout.ExpandWidth(true));
            GUILayout.EndHorizontal();

            scroll = GUILayout.BeginScrollView(scroll);
            for (int i = 0; i < _source.Count; i++)
            {
                string name = String.Empty;

                if (_source[i] is UnityEngine.Object basic)
                {
                    name = basic.name;
                }
                else
                {
                    name = _source[i].ToString();
                }

                if (search.IsNullOrEmpty() is false && search.Contains(name) is false)
                {
                    continue;
                }

                isSelect[i] = GUILayout.Toggle(isSelect[i], name);
            }

            GUILayout.EndScrollView();
        }
    }
}