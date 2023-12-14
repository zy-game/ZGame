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
    public class PageScene : IDisposable
    {
        public string name { get; private set; }
        public PageScene parent { get; private set; }
        public Rect position { get; set; }
        public string search { get; set; }

        private Dictionary<IEnumerator, EditorCoroutine> coroutines = new Dictionary<IEnumerator, EditorCoroutine>();

        public PageScene()
        {
            Init();
        }

        void Init()
        {
            BindScene attribute = this.GetType().GetCustomAttribute<BindScene>();
            if (attribute is null)
            {
                return;
            }

            this.name = attribute.name;
            if (attribute.parent is null)
            {
                return;
            }

            this.parent = EditorManager.GetScene(attribute.parent);
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

        public bool OnShowFoldoutHeader(string name, bool show2)
        {
            Rect rect = EditorGUILayout.BeginHorizontal(ZStyle.GUI_STYLE_BOX_BACKGROUND);
            show2 = EditorGUILayout.Foldout(show2, "");
            GUILayout.Space(-40);
            EditorGUILayout.LabelField(name, EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();
            if (Event.current.type == EventType.MouseDown && rect.Contains(Event.current.mousePosition))
            {
                show2 = !show2;
                Event.current.Use();
            }

            GUILayout.EndHorizontal();
            return show2;
        }

        public virtual void OnEnable()
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