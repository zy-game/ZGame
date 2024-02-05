using System;
using System.Collections.Generic;
using PhotoshopFile;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ZGame.Editor.PSD2GUI
{
    [SubPageSetting("PSD转UI")]
    public class PSD2GUIWindow : SubPage
    {
        public override void SearchRightDrawing()
        {
            if (GUILayout.Button(EditorGUIUtility.IconContent(ZStyle.ADD_BUTTON_ICON), ZStyle.HEADER_BUTTON_STYLE))
            {
                PSDConfig.instance.imports.Add(new PSDImport());
            }
        }

        public override void OnGUI()
        {
            if (EditorApplication.isPlaying)
            {
                return;
            }

            GUILayout.BeginVertical();
            EditorGUILayout.Space(5);
            for (int i = 0; i < PSDConfig.instance.imports.Count; i++)
            {
                var import = PSDConfig.instance.imports[i];
                GUILayout.BeginHorizontal(ZStyle.BOX_BACKGROUND);
                import.psd = EditorGUILayout.ObjectField(import.psd, typeof(Object), false);
                GUILayout.FlexibleSpace();
                if (GUILayout.Button(EditorGUIUtility.IconContent(ZStyle.SETTING_BUTTON_ICON), ZStyle.HEADER_BUTTON_STYLE, GUILayout.ExpandWidth(false)))
                {
                    EditorManager.SwitchScene<PSDImportEditorWindow>(import);
                }

                if (GUILayout.Button(EditorGUIUtility.IconContent(ZStyle.DELETE_BUTTON_ICON), ZStyle.HEADER_BUTTON_STYLE))
                {
                    PSDConfig.instance.imports.Remove(import);
                    EditorManager.Refresh();
                }

                if (GUILayout.Button(EditorGUIUtility.IconContent(ZStyle.PLAY_BUTTON_ICON), ZStyle.HEADER_BUTTON_STYLE))
                {
                    import.Export();
                }

                GUILayout.EndHorizontal();
            }

            EditorGUILayout.EndVertical();
        }
    }
}