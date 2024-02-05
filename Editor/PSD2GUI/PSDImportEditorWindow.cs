using System;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ZGame.Editor.PSD2GUI
{
    [SubPageSetting("PSD Editor", typeof(PSD2GUIWindow), true)]
    public sealed class PSDImportEditorWindow : SubPage
    {
        private PSDImport import;
        private Vector2 hierarchyScrollPosition;
        private Vector2 inspectorScrollPosition;
        private Vector2 scaleOffset;
        private Vector2 posOffset;
        private Rect rectDraw;
        private object dragData = "dragData";
        private Vector2 offset;
        private Color col = new Color(1, 0, 0, 0.6f);
        private Rect drect;
        private bool isDraging;

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
            if (GUILayout.Button("Refresh", EditorStyles.toolbarButton))
            {
                import.Refresh();
            }

            if (GUILayout.Button("Generic", EditorStyles.toolbarButton))
            {
                import.Export();
            }

            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            rectDraw = EditorGUILayout.BeginVertical(ZStyle.ITEM_BACKGROUND_STYLE, GUILayout.Width(300));
            hierarchyScrollPosition = GUILayout.BeginScrollView(hierarchyScrollPosition);
            OnDrawingHierarchy();
            GUILayout.EndScrollView();
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndVertical();
            GUILayout.FlexibleSpace();

            for (int i = 0; i < import.layers.Count; i++)
            {
                Drawlayer(import.layers[i]);
            }

            GUILayout.FlexibleSpace();
            GUILayout.BeginVertical(ZStyle.ITEM_BACKGROUND_STYLE, GUILayout.Width(300));
            inspectorScrollPosition = GUILayout.BeginScrollView(inspectorScrollPosition);
            OnDrawingInspector();
            GUILayout.EndScrollView();
            GUILayout.FlexibleSpace();
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
            OnCheckOpertion();
            OnDrawingDragLayer();
        }

        private void OnCheckOpertion()
        {
        }

        private PSDLayer curLayer;

        private void OnDrawingHierarchy()
        {
            for (int i = 0; i < import.layers.Count; i++)
            {
                OnDrawingPSDLayerHierarchy(import.layers[i], 0);
            }
        }

        private void OnDrawingPSDLayerHierarchy(PSDLayer layer, float dep)
        {
            GUIStyle style = curLayer is not null && curLayer.Equals(layer) ? EditorStyles.selectionRect : GUIStyle.none;
            layer.rectDraw = EditorGUILayout.BeginHorizontal(style, GUILayout.Height(25));
            GUILayout.Space(dep * 15);
            if (layer.children.Count > 0)
            {
                layer.isOn = EditorGUILayout.Foldout(layer.isOn, string.Empty);
                GUILayout.Space(-40);
            }

            GUILayout.Label(layer.texture, GUILayout.Width(20), GUILayout.Height(18));
            GUILayout.Label(layer.name);
            GUILayout.FlexibleSpace();
            if (GUILayout.Button(EditorGUIUtility.IconContent(layer.active ? "ViewToolOrbit On" : "d_scenevis_hidden_hover"), ZStyle.HEADER_BUTTON_STYLE))
            {
                layer.active = !layer.active;
            }

            if (this.OnMouseLeftButtonDown(layer.rectDraw))
            {
                curLayer = layer;
                drect = layer.rectDraw;
                offset = layer.rectDraw.position - Event.current.mousePosition;
                EditorManager.Refresh();
            }

            GUILayout.EndHorizontal();

            if (layer.children is null || layer.children.Count == 0 || layer.isOn is false)
            {
                return;
            }

            foreach (var VARIABLE in layer.children)
            {
                OnDrawingPSDLayerHierarchy(VARIABLE, ++dep);
            }
        }


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
                    DragAndDrop.PrepareStartDrag();
                    DragAndDrop.SetGenericData("dragflag", dragData);
                    DragAndDrop.StartDrag("dragtitle");
                    isDraging = true;
                    e.Use();
                    break;
                case EventType.DragUpdated:
                    drect = new Rect(e.mousePosition + offset, curLayer.rectDraw.size);
                    if (import.layers.Exists(x => x.isJoinDrawingRect(drect.position) && x.Equals(curLayer) is false) || rectDraw.Contains(drect.position))
                    {
                        DragAndDrop.visualMode = DragAndDropVisualMode.Generic;
                    }

                    e.Use();
                    break;
                case EventType.DragPerform:
                    PSDLayer to = import.layers.Find(x => x.rectDraw.Contains(drect.position) && x.Equals(curLayer) is false);
                    if (to is not null)
                    {
                        to.children.Add(curLayer);

                        if (curLayer.parent is not null)
                        {
                            curLayer.parent.children.Remove(curLayer);
                        }

                        curLayer.parent = to;
                        import.layers.Remove(curLayer);
                        EditorManager.Refresh();
                    }
                    else if (rectDraw.Contains(drect.position))
                    {
                        import.layers.Add(curLayer);
                        if (curLayer.parent is not null)
                        {
                            curLayer.parent.children.Remove(curLayer);
                        }

                        curLayer.parent = null;
                    }

                    DragAndDrop.AcceptDrag();
                    e.Use();
                    break;
                case EventType.DragExited:
                    isDraging = false;
                    e.Use();
                    break;
                case EventType.MouseUp:
                    curLayer = null;
                    break;
            }

            if (isDraging)
            {
                GUI.Box(drect, String.Empty, ZStyle.ITEM_BACKGROUND_STYLE);
                Vector2 pos = new Vector2(drect.x, drect.y);
                Vector2 size = Vector2.one * 20;
                Rect rect = new Rect(pos, size);
                GUI.Label(rect, curLayer.texture);
                GUI.Label(new Rect(pos + new Vector2(30, 0), drect.size - new Vector2(30, 0)), curLayer.name);
            }
        }

        private void OnDrawingInspector()
        {
        }

        private void Drawlayer(PSDLayer layer)
        {
            if (layer.children.Count > 0 && layer.active)
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