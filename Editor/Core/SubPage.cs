using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ZGame.Editor
{
    public class SubPage : IDisposable
    {
        public string name { get; private set; }
        public SubPage parent { get; private set; }
        public List<SubPage> childs { get; private set; }
        public bool show;
        public Rect position { get; set; }
        public string search { get; set; }

        private Type referenceType;
        private Dictionary<IEnumerator, EditorCoroutine> coroutines = new Dictionary<IEnumerator, EditorCoroutine>();
        private static event Func<Type, bool> OnOpenAssetCallback;

        public SubPage()
        {
            ReferenceScriptableObject reference = this.GetType().GetCustomAttribute<ReferenceScriptableObject>();
            if (reference is not null)
            {
                referenceType = reference.type;
                OnOpenAssetCallback += OnOpenAsset;
            }

            childs = new List<SubPage>();
            SubPageSetting attribute = this.GetType().GetCustomAttribute<SubPageSetting>();
            if (attribute is null)
            {
                return;
            }

            this.name = attribute.name;
        }

        private bool OnOpenAsset(Type type)
        {
            if (type.Equals(referenceType) is false)
            {
                return false;
            }

            EditorManager.SwitchScene(this);
            return true;
        }


        [OnOpenAsset()]
        static bool OnOpened(int id, int line)
        {
            UnityEngine.Object target = UnityEditor.EditorUtility.InstanceIDToObject(id);
            if (target == null)
            {
                return false;
            }

            if (OnOpenAssetCallback is null)
            {
                return false;
            }

            return OnOpenAssetCallback(target.GetType());
        }

        public EditorCoroutine StartCoroutine(IEnumerator enumerator)
        {
            IEnumerator OnStart()
            {
                yield return enumerator;
                coroutines.Remove(enumerator);
            }

            var coroutine = EditorCoroutineUtility.StartCoroutine(OnStart(), this);
            coroutines.Add(enumerator, coroutine);
            return coroutine;
        }

        public void StopCoroutine(IEnumerator enumerator)
        {
            if (coroutines.TryGetValue(enumerator, out var coroutine))
            {
                EditorCoroutineUtility.StopCoroutine(coroutine);
                coroutines.Remove(enumerator);
            }
        }

        public void StopAllCoroutine()
        {
            foreach (var coroutine in coroutines.Values)
            {
                EditorCoroutineUtility.StopCoroutine(coroutine);
            }

            coroutines.Clear();
        }

        public virtual void DrawingFoldoutHeaderRight(object userData)
        {
        }

        Color temp = Color.white;

        public void OnDrawingSplitLine(float width, Color color)
        {
            GUILayout.Space(2);
            temp = GUI.color;
            GUI.color = color;
            GUILayout.Box("", ZStyle.GUI_STYLE_LINE, GUILayout.Height(1), GUILayout.Width(width));
            GUI.color = temp;
        }

        public bool OnShowFoldoutHeader(string name, bool isOn, object userData = null)
        {
            Rect rect = EditorGUILayout.BeginHorizontal(ZStyle.BOX_BACKGROUND);
            isOn = EditorGUILayout.Foldout(isOn, "");
            GUILayout.Space(-40);
            EditorGUILayout.LabelField(name, EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();
            DrawingFoldoutHeaderRight(userData);
            if (Event.current.type == EventType.MouseDown && rect.Contains(Event.current.mousePosition))
            {
                isOn = !isOn;
                Event.current.Use();
            }

            GUILayout.EndHorizontal();
            return isOn;
        }

        public virtual void SearchRightDrawing()
        {
        }

        public virtual void OnEnable(params object[] args)
        {
        }

        public virtual void OnDisable()
        {
        }

        public virtual void OnGUI()
        {
        }

        public virtual void Dispose()
        {
        }
    }
}