using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TMPro;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UI;
using ZGame.Config;
using ZGame.UI;
using Object = UnityEngine.Object;

namespace ZGame.Editor.UIBind
{
    [CustomEditor(typeof(UI.UIDocment))]
    [CanEditMultipleObjects]
    public class UIDocmentInspector : UnityEditor.Editor
    {
        private bool isTemplete;
        private UI.UIDocment docment;


        void OnEnable()
        {
            this.docment = (UI.UIDocment)target;
            this.isTemplete = this.docment.transform.parent?.GetComponentInParent<UIDocment>() != null;
        }

        void OnDisable()
        {
        }

        public override void OnInspectorGUI()
        {
            if (isTemplete is false)
            {
                EditorGUILayout.PropertyField(this.serializedObject.FindProperty("NameSpace"), new GUIContent("命名空间"));
                EditorGUILayout.PropertyField(this.serializedObject.FindProperty("output"), new GUIContent("输出路径"));
            }

            GUILayout.Space(10);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Bind List", EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();
            if (GUILayout.Button(EditorGUIUtility.FindTexture(ZStyle.ADD_BUTTON_ICON), GUIStyle.none))
            {
                this.docment.bindList.Add(null);
                this.Repaint();
            }

            EditorGUILayout.EndHorizontal();
            GUILayoutTools.DrawLine(ZStyle.inColor);
            GUILayout.Space(5);
            Rect rect = EditorGUILayout.BeginVertical();
            if (this.docment.bindList is not null || this.docment.bindList.Count > 0)
            {
                for (int i = 0; i < this.docment.bindList.Count; i++)
                {
                    EditorGUILayout.BeginHorizontal(ZStyle.BOX_BACKGROUND);
                    this.docment.bindList[i] = (GameObject)EditorGUILayout.ObjectField("", this.docment.bindList[i], typeof(GameObject), true);
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button(EditorGUIUtility.FindTexture(ZStyle.DELETE_BUTTON_ICON), EditorStyles.toolbarButton))
                    {
                        this.docment.bindList.RemoveAt(i);
                        this.Repaint();
                    }

                    EditorGUILayout.EndHorizontal();
                    GUILayout.Space(2);
                }
            }

            EditorGUILayout.EndVertical();

            if (UnityEngine.Event.current.type == EventType.DragUpdated && rect.Contains(UnityEngine.Event.current.mousePosition))
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Link;
            }

            if (UnityEngine.Event.current.type == EventType.DragPerform && rect.Contains(UnityEngine.Event.current.mousePosition))
            {
                DragAndDrop.AcceptDrag();
                foreach (UnityEngine.Object obj in DragAndDrop.objectReferences)
                {
                    if (obj is GameObject gameObject)
                    {
                        docment.bindList.RemoveAll(x => x == gameObject);
                        docment.bindList.Add(gameObject);
                    }
                }

                this.serializedObject.ApplyModifiedProperties();
            }
        }
    }
}