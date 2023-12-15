using System;
using System.Collections.Generic;
using Unity.Entities;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using ZGame.Game.Baker;

namespace ZGame.Editor.Baker
{
    [CustomEditor(typeof(MultiAutoBaker))]
    public class MultiBakerEditor : CustomEditorWindow
    {
        private MultiAutoBaker baker;
        private List<Type> componentDatas;

        private void OnEnable()
        {
            baker = target as MultiAutoBaker;
            if (baker.bakerDataList == null)
            {
                baker.bakerDataList = new List<BakerData>();
            }

            componentDatas = AppDomain.CurrentDomain.GetAllSubClasses<IComponentData>();
        }

        public override void OnInspectorGUI()
        {
            if (baker.bakerDataList == null || baker.bakerDataList.Count == 0)
            {
                return;
            }

            EditorGUI.BeginChangeCheck();
            baker.asset = (AssemblyDefinitionAsset)EditorGUILayout.ObjectField("Assembly", baker.asset, typeof(AssemblyDefinitionAsset), false);

            for (int i = 0; i < baker.bakerDataList.Count; i++)
            {
                BakerData bakerData = baker.bakerDataList[i];
                GUILayout.BeginHorizontal();
                bakerData.gameObject = (GameObject)EditorGUILayout.ObjectField(bakerData.gameObject, typeof(GameObject), true);
                if (bakerData.gameObject == null)
                {
                    GUILayout.EndHorizontal();
                    continue;
                }

                if (bakerData.components == null)
                {
                    bakerData.components = new List<string>();
                }

                if (bakerData.componentList == null)
                {
                    bakerData.componentList = new List<Type>();
                    for (int j = 0; j < bakerData.components.Count; j++)
                    {
                        bakerData.componentList.Add(AppDomain.CurrentDomain.GetType(bakerData.components[j]));
                    }
                }


                GUIContent content = new GUIContent("Component");
                if (GUILayout.Button(content, EditorStyles.popup))
                {
                    Rect rect = new Rect(UnityEngine.Event.current.mousePosition, Vector2.zero);
                    ObjectSelectionWindow.Show(rect, bakerData.componentList, componentDatas, () =>
                    {
                        bakerData.components.Clear();
                        for (int j = 0; j < bakerData.componentList.Count; j++)
                        {
                            bakerData.components.Add(bakerData.componentList[j].FullName);
                        }

                        EditorUtility.SetDirty(baker);
                    });
                }

                GUILayout.FlexibleSpace();
                if (GUILayout.Button(String.Empty, ZStyle.GUI_STYLE_MINUS))
                {
                    baker.bakerDataList.Remove(bakerData);
                    Repaint();
                }

                GUILayout.EndHorizontal();
            }

            if (GUILayout.Button("Add"))
            {
                baker.bakerDataList.Add(new BakerData());
            }

            if (EditorGUI.EndChangeCheck())
            {
                this.serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(baker);
            }
        }
    }
}