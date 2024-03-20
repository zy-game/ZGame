using System;
using System.Collections.Generic;
using PhotoshopFile;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ZGame.Editor.PSD2GUI
{
    [GameSubEditorWindowOptions("PSD转UI")]
    public class PSD2GUIWindow : GameSubEditorWindow
    {
        public override void SearchRightDrawing()
        {
            if (GUILayout.Button(EditorGUIUtility.IconContent(ZStyle.ADD_BUTTON_ICON), ZStyle.HEADER_BUTTON_STYLE))
            {
                PSDConfig.instance.options.Add(new PSDOptions());
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
            for (int i = 0; i < PSDConfig.instance.options.Count; i++)
            {
                var import = PSDConfig.instance.options[i];
                GUILayout.BeginHorizontal(ZStyle.BOX_BACKGROUND);
                import.psd = EditorGUILayout.ObjectField(import.psd, typeof(Object), false);
                GUILayout.FlexibleSpace();
                if (GUILayout.Button(EditorGUIUtility.IconContent(ZStyle.SETTING_BUTTON_ICON), ZStyle.HEADER_BUTTON_STYLE, GUILayout.ExpandWidth(false)))
                {
                    GameBaseEditorWindow.SwitchScene<PSDImportEditorWindow>(import);
                }

                if (GUILayout.Button(EditorGUIUtility.IconContent(ZStyle.DELETE_BUTTON_ICON), ZStyle.HEADER_BUTTON_STYLE))
                {
                    PSDConfig.instance.options.Remove(import);
                    GameBaseEditorWindow.Refresh();
                }

                if (GUILayout.Button(EditorGUIUtility.IconContent(ZStyle.PLAY_BUTTON_ICON), ZStyle.HEADER_BUTTON_STYLE))
                {
                    import.Export();
                }

                GUILayout.EndHorizontal();
            }

            EditorGUILayout.EndVertical();
        }

        public override void SaveChanges()
        {
            PSDConfig.Save();
        }
    }
}