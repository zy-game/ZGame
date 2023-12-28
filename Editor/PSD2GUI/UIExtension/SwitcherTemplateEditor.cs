using System;
using UnityEditor;
using UnityEngine;
using ZGame.Window;

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
        }
    }
}