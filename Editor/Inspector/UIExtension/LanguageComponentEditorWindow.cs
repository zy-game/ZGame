using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using ZGame.Config;
using ZGame.UI;

namespace ZGame.Editor.UIExtension
{
    [CustomEditor(typeof(LanguageComponent))]
    public class LanguageComponentEditorWindow : UnityEditor.Editor
    {
        public LanguageComponent component;

        void OnEnable()
        {
            component = (LanguageComponent)target;
        }

        public override void OnInspectorGUI()
        {
            if (EditorGUILayout.DropdownButton(new GUIContent(LanguageConfig.instance.Query(component.languageCode)), FocusType.Passive))
            {
                SelectorWindow.Show(GetTreeItem(), x => { component.languageCode = (int)x.FirstOrDefault().userData; });
            }
        }

        private IEnumerable<SelectorItem> GetTreeItem()
        {
            return LanguageConfig.instance.config.Select(x => new SelectorItem(null, x.zh, x.id));
        }
    }
}