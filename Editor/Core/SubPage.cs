﻿using System;
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
        private bool isOn;
        private Type referenceType;
        private Color temp = Color.white;
        private Dictionary<IEnumerator, EditorCoroutine> coroutines = new();
        private static event Func<Type, bool> OnOpenAssetCallback;


        public string name { get; private set; }
        public List<SubPage> childs { get; private set; }
        public Rect position { get; set; }
        public string search { get; set; }

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

        public bool IsSelection(SubPage page)
        {
            return this.Equals(page) || this.childs.Contains(page);
        }

        public void OnDrawingMeunItem(SubPage current, float offset)
        {
            Rect contains = EditorGUILayout.BeginVertical();
            GUILayout.BeginHorizontal();
            {
                GUILayout.Space(offset);
                this.BeginColor(IsSelection(current) ? Color.cyan : GUI.color);
                if (childs.Count > 0)
                {
                    GUILayout.BeginVertical(GUILayout.Width(20));
                    GUILayout.Space(3);
                    isOn = EditorGUILayout.Foldout(isOn, "");
                    GUILayout.EndVertical();
                    GUILayout.Space(-40);
                }

                GUILayout.Label(name, ZStyle.GUI_STYLE_TITLE_LABLE);
                this.EndColor();
                GUILayout.EndHorizontal();
            }

            GUILayout.Space(5);
            OnDrawingSplitLine(300, ZStyle.outColor);
            if (this.OnMouseLeftButtonDown(contains))
            {
                EditorManager.SwitchScene(this);
            }

            GUILayout.EndVertical();

            if (childs.Count > 0 && isOn)
            {
                foreach (var VARIABLE in childs)
                {
                    VARIABLE.OnDrawingMeunItem(current, 15);
                }
            }
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

        public virtual void OnDrawingHeaderRight(object userData)
        {
        }

        public virtual void OnShowHeaderRightMeun(object userData)
        {
        }

        public void OnDrawingSplitLine(float width, Color color)
        {
            temp = GUI.color;
            GUI.color = color;
            GUILayout.Box("", ZStyle.GUI_STYLE_LINE, GUILayout.Height(1), GUILayout.Width(width));
            GUI.color = temp;
        }

        public bool OnBeginHeader(string name, bool isOn, object userData = null, bool isFoldout = true)
        {
            Rect rect = EditorGUILayout.BeginHorizontal();
            GUILayout.Space(2);
            EditorGUILayout.BeginHorizontal("ContentToolbar");
            if (isFoldout)
            {
                isOn = EditorGUILayout.Foldout(isOn, String.Empty);
                GUILayout.Space(-40);
            }

            EditorGUILayout.LabelField(name, EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();
            OnDrawingHeaderRight(userData);
            if (this.OnMouseLeftButtonDown(rect))
            {
                isOn = !isOn;
                Event.current.Use();
            }

            if (this.OnMouseRightButtomDown(rect))
            {
                OnShowHeaderRightMeun(userData);
                Event.current.Use();
            }

            GUILayout.EndHorizontal();
            GUILayout.Space(2);
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