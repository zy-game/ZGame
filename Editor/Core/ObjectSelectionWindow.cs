using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ZGame.Editor
{
    public enum SelectionType
    {
        Single,
        Multiple
    }

    public class ObjectSelectionWindow<T> : PopupWindowContent
    {
        private bool[] isSelected;
        private List<T> _source;
        private List<T> _selection;
        private string search;
        private Vector2 scroll;
        private Action<List<T>> _multipleAction;
        private Action<T> _singleAction;
        private SelectionType _type;
        private Vector2 _size;
        private EditorWindow window;

        public static void ShowSingle(Vector2 size, List<T> source, Action<T> closeCallback = null)
        {
            ObjectSelectionWindow<T> selectionWindow = new ObjectSelectionWindow<T>();
            selectionWindow._source = source;
            selectionWindow.isSelected = new bool[source.Count];
            selectionWindow._singleAction = closeCallback;
            selectionWindow._type = SelectionType.Single;
            selectionWindow.search = "";
            selectionWindow._size = size;
            PopupWindow.Show(new Rect(UnityEngine.Event.current.mousePosition, Vector2.zero), selectionWindow);
        }

        public static void ShowMultiple(Vector2 size, List<T> selection, List<T> source, Action<List<T>> closeCallback = null)
        {
            ObjectSelectionWindow<T> selectionWindow = new ObjectSelectionWindow<T>();
            selectionWindow._source = source;
            selectionWindow.isSelected = new bool[source.Count];
            selectionWindow._selection = selection;
            selectionWindow._multipleAction = closeCallback;
            selectionWindow._type = SelectionType.Multiple;
            selectionWindow.search = "";
            selectionWindow._size = size;
            PopupWindow.Show(new Rect(UnityEngine.Event.current.mousePosition, Vector2.zero), selectionWindow);
        }

        public override Vector2 GetWindowSize()
        {
            return _size;
        }

        public override void OnOpen()
        {
            if (_type == SelectionType.Multiple)
            {
                for (int i = 0; i < _source.Count; i++)
                {
                    isSelected[i] = _selection.Contains(_source[i]);
                }
            }
        }

        public override void OnClose()
        {
            if (_type == SelectionType.Multiple)
            {
                _selection.Clear();
                for (int i = 0; i < isSelected.Length; i++)
                {
                    if (isSelected[i])
                    {
                        _selection.Add(_source[i]);
                    }
                }

                _multipleAction?.Invoke(_selection);
            }
        }

        public override void OnGUI(Rect rect)
        {
            if (_source == null || _source.Count == 0)
            {
                return;
            }

            GUILayout.BeginHorizontal(ZStyle.BOX_BACKGROUND);
            EditorGUILayout.LabelField("Selection", EditorStyles.boldLabel, GUILayout.ExpandWidth(true));
            GUILayout.EndHorizontal();
            this.BeginColor(ZStyle.inColor);
            GUILayout.Box("", ZStyle.GUI_STYLE_LINE, GUILayout.Height(1));
            this.EndColor();
            GUILayout.BeginHorizontal();
            search = GUILayout.TextField(search, GUILayout.ExpandWidth(true));
            GUILayout.EndHorizontal();
            GUILayout.Space(5);
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

                Rect rect2 = EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
                if (_type == SelectionType.Multiple)
                {
                    isSelected[i] = GUILayout.Toggle(isSelected[i], name);
                }
                else
                {
                    EditorGUILayout.LabelField(name, EditorStyles.boldLabel);
                    if (Event.current.type == EventType.MouseUp && rect2.Contains(Event.current.mousePosition))
                    {
                        _singleAction?.Invoke(_source[i]);
                        Event.current.Use();
                        EditorWindow.focusedWindow.Close();
                    }
                }

                EditorGUILayout.EndHorizontal();
            }

            GUILayout.EndScrollView();
        }
    }
}