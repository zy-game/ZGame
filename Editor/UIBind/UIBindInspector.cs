using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Sirenix.OdinInspector.Editor;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using ZGame.Config;
using ZGame.UI;
using Object = UnityEngine.Object;

namespace ZGame.Editor.PSD2GUI
{
    [CustomEditor(typeof(UIBind))]
    public class UIBindInspector : OdinEditor
    {
        private UIBind setting;
        private bool isSetLanguage = false;

        public void OnEnable()
        {
            this.setting = (UIBind)target;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (this.setting.isTemplete())
            {
                return;
            }

            if (EditorGUILayout.DropdownButton(new GUIContent("Generic"), FocusType.Passive))
            {
                if (setting.output == null)
                {
                    EditorUtility.DisplayDialog("Error", "Please select output path", "OK");
                    return;
                }

                if (setting.bindList.Count == 0)
                {
                    EditorUtility.DisplayDialog("Error", "Please select bind list", "OK");
                    return;
                }

                GenericMenu menu = new GenericMenu();
                menu.AddItem(new GUIContent("Generic UIBind"), false, () => { UIBindRulerConfig.instance.GenericUIBindCode(setting, false); });
                menu.AddItem(new GUIContent("Generic UIBind And UICode"), false, () => { UIBindRulerConfig.instance.GenericUIBindCode(setting, true); });
                menu.ShowAsContext();
            }
        }
    }
}