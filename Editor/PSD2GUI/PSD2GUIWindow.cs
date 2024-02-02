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
        private Vector2 hierarchyScrollPosition;
        private Vector2 inspectorScrollPosition;
        private Vector2 scaleOffset;
        private Vector2 posOffset;

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

            GUILayout.BeginHorizontal(ZStyle.ITEM_BACKGROUND_STYLE);
            EditorGUI.BeginDisabledGroup(true);
            import.psd = EditorGUILayout.ObjectField(import.psd, typeof(Object), false);
            EditorGUI.EndDisabledGroup();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Refresh"))
            {
                import.Refresh();
            }

            OnDrawingDragLayer();
            OnCheckOpertion();
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical(ZStyle.BOX_BACKGROUND, GUILayout.Width(300));
            hierarchyScrollPosition = GUILayout.BeginScrollView(hierarchyScrollPosition);
            OnDrawingHierarchy();
            GUILayout.EndScrollView();
            GUILayout.FlexibleSpace();
            GUILayout.EndVertical();
            GUILayout.FlexibleSpace();

            OnDrawingUIPreviewScene();

            GUILayout.FlexibleSpace();
            GUILayout.BeginVertical(ZStyle.BOX_BACKGROUND, GUILayout.Width(300));
            inspectorScrollPosition = GUILayout.BeginScrollView(inspectorScrollPosition);
            OnDrawingInspector();
            GUILayout.EndScrollView();
            GUILayout.FlexibleSpace();
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
        }

        private void OnCheckOpertion()
        {
        }

        private PSDLayer curLayer;
        private Rect dragRect;

        private void OnDrawingHierarchy()
        {
            for (int i = 0; i < import.layers.Count; i++)
            {
                PSDLayer layer = import.layers[i];
                Rect rect = EditorGUILayout.BeginHorizontal(curLayer is not null && curLayer.Equals(layer) ? EditorStyles.selectionRect : ZStyle.ITEM_BACKGROUND_STYLE);
                GUILayout.Label(layer.texture, GUILayout.Width(20), GUILayout.Height(20));
                GUILayout.Label(layer.name);
                GUILayout.FlexibleSpace();
                if (GUILayout.Button(EditorGUIUtility.IconContent(layer.active ? "ViewToolOrbit On" : "d_scenevis_hidden_hover"), ZStyle.HEADER_BUTTON_STYLE))
                {
                    layer.active = !layer.active;
                }

                if (this.OnMouseLeftButtonDown(rect))
                {
                    curLayer = layer;
                    dragRect = rect;
                    GUIUtility.hotControl = EditorGUIUtility.GetControlID(FocusType.Passive, rect);
                    Debug.Log("click");
                    EditorManager.Refresh();
                }

                GUILayout.EndHorizontal();
            }
        }

        object dragData = "dragData";
        Vector2 offset;
        Color col = new Color(1, 0, 0, 0.6f);
        Rect drect;
        bool isDraging;
        int cid;

        private void OnDrawingDragLayer()
        {
            //判断是否拖拽
            if (curLayer is null)
            {
                return;
            }

            Event e = Event.current;
            switch (e.type)
            {
                case EventType.MouseDrag:
                    if (dragRect.Contains(e.mousePosition))
                    {
                        Debug.Log("MouseDrag");
                        DragAndDrop.PrepareStartDrag();
                        //DragAndDrop.objectReferences = new Object[] { };
                        DragAndDrop.SetGenericData("dragflag", dragData);
                        DragAndDrop.StartDrag("dragtitle");
                        offset = e.mousePosition - dragRect.position;
                        drect = dragRect;
                        isDraging = true;
                        e.Use();
                    }

                    break;
                case EventType.DragUpdated:
                    Debug.Log("DragUpdated");
                    drect.position = e.mousePosition - offset;
                    // if (rect2.Contains(e.mousePosition))
                    // {
                    //     DragAndDrop.visualMode = DragAndDropVisualMode.Generic;
                    //     drect = rect2;
                    // }

                    e.Use();
                    break;
                case EventType.DragPerform:
                    Debug.Log("DragPerform");
                    DragAndDrop.AcceptDrag();
                    Debug.Log("DragPerform : " + DragAndDrop.GetGenericData("dragflag"));
                    e.Use();
                    break;
                case EventType.DragExited:
                    Debug.Log("DragExited");
                    isDraging = false;
                    if (GUIUtility.hotControl == cid)
                        GUIUtility.hotControl = 0;
                    e.Use();
                    break;
            }

            if (isDraging)
            {
                EditorGUI.DrawRect(drect, Color.blue);
            }
        }

        private void OnDrawingInspector()
        {
        }

        private void OnDrawingUIPreviewScene()
        {
            for (int i = 0; i < import.layers.Count; i++)
            {
                Drawlayer(import.layers[i]);
            }
        }

        private void Drawlayer(PSDLayer layer)
        {
            if (layer.children.Count > 0)
            {
                for (int i = 0; i < layer.children.Count; i++)
                {
                    Drawlayer(layer.children[i]);
                }
            }

            if (layer.active is false)
            {
                return;
            }

            Vector2 yoffset = new Vector2(300, 50);
            Vector2 pos = layer.rect.position / 2f + yoffset;
            Vector2 size = layer.rect.size / 2f + scaleOffset;
            Rect rect = new Rect(pos, size);
            GUI.Label(rect, layer.texture);
        }
    }
}