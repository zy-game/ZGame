using System;
using UnityEditor;
using UnityEngine;
using ZGame.UI;

namespace ZGame.Editor.PSD2GUI
{
    [CustomEditor(typeof(UISwitcherTemplate))]
    public class SwitcherTemplateEditor : CustomEditorWindow
    {
        private UISwitcherTemplate template;

        public void OnEnable()
        {
            template = target as UISwitcherTemplate;
        }

        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();
            template.type = (SwitchType2)EditorGUILayout.EnumPopup("Type", template.type);
            switch (template.type)
            {
                case SwitchType2.Sprite:
                    template.options.activeSprite = (Sprite)EditorGUILayout.ObjectField("Active", template.options.activeSprite, typeof(Sprite), false);
                    template.options.inactiveSprite = (Sprite)EditorGUILayout.ObjectField("Inactive", template.options.inactiveSprite, typeof(Sprite), false);
                    break;
                case SwitchType2.Text:
                    template.options.activeText = EditorGUILayout.TextField("Active", template.options.activeText);
                    template.options.inactiveText = EditorGUILayout.TextField("Inactive", template.options.inactiveText);
                    break;
                case SwitchType2.GameObject:
                    template.options.gameObject = (GameObject)EditorGUILayout.ObjectField("GameObject", template.options.gameObject, typeof(GameObject), false);
                    break;
            }

            template.paramType = (ParamType)EditorGUILayout.EnumPopup("ParamType", template.paramType);
            switch (template.paramType)
            {
                case ParamType.Int:

                    template._v1 = (int)EditorGUILayout.IntField("Int", template._v1);
                    break;
                case ParamType.Float:
                    template._v2 = (float)EditorGUILayout.FloatField("Float", template._v2);
                    break;
                case ParamType.String:
                    template._v3 = (string)EditorGUILayout.TextField("String", template._v3);
                    break;
                case ParamType.Bool:
                    template._v4 = (bool)EditorGUILayout.Toggle("Bool", template._v4);
                    break;
                case ParamType.Vector2:
                    template._v5 = (Vector2)EditorGUILayout.Vector2Field("Vector2", template._v5);
                    break;
                case ParamType.Vector3:
                    template._v6 = (Vector3)EditorGUILayout.Vector3Field("Vector3", template._v6);
                    break;
                case ParamType.Vector4:
                    template._v7 = (Vector4)EditorGUILayout.Vector4Field("Vector4", template._v7);
                    break;
                case ParamType.Color:
                    template._v8 = (Color)EditorGUILayout.ColorField("Color", template._v8);
                    break;
            }

            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(template);
            }
        }
    }
}