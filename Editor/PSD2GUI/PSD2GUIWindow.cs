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
                    EditorManager.SwitchScene<ImportEditorPage>(import);
                }

                if (GUILayout.Button(EditorGUIUtility.IconContent(ZStyle.DELETE_BUTTON_ICON), ZStyle.HEADER_BUTTON_STYLE))
                {
                    PSDConfig.instance.imports.Remove(import);
                    EditorManager.Refresh();
                }

                GUILayout.EndHorizontal();
            }

            EditorGUILayout.EndVertical();
        }
    }

    [SubPageSetting("PSD Editor", typeof(PSD2GUIWindow))]
    public sealed class ImportEditorPage : SubPage
    {
        private PSDImport import;

        public override void OnEnable(params object[] args)
        {
            if (args == null || args.Length == 0)
            {
                return;
            }

            import = args[0] as PSDImport;
            if (import.layers is null)
            {
                import.layers = new();
            }
        }

        public override void OnGUI()
        {
            if (import is null)
            {
                return;
            }

            GUILayout.BeginHorizontal();
            EditorGUI.BeginDisabledGroup(true);
            import.psd = EditorGUILayout.ObjectField(import.psd, typeof(Object), false);
            EditorGUI.EndDisabledGroup();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Refresh"))
            {
                import.Refresh();
            }

            GUILayout.EndHorizontal();
            for (int i = 0; i < import.layers.Count; i++)
            {
                Drawlayer(import.layers[i]);
            }
        }

        private void Drawlayer(PSDLayer layer)
        {
            if (layer.children.Count == 0)
            {
                GUILayout.Label(layer.name);
                return;
            }

            layer.isOn = OnBeginHeader(layer.name, layer.isOn);
            if (layer.isOn)
            {
                for (int i = 0; i < layer.children.Count; i++)
                {
                    Drawlayer(layer.children[i]);
                }
            }
        }
    }
}