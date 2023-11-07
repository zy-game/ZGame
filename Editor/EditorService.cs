using System;
using UnityEditor;
using UnityEngine;

namespace ZGame.Editor
{
    public class EditorService
    {
    }

    class ZStyle
    {
        public static readonly GUIContent empty = EditorGUIUtility.TrTextContent(String.Empty);
        public static readonly GUIContent offsetLabel = EditorGUIUtility.TrTextContent("地块偏移");
        public static readonly GUIContent rotationLabel = EditorGUIUtility.TrTextContent("地块旋转");
        public static readonly GUIContent scaleLabel = EditorGUIUtility.TrTextContent("地块缩放");
        public static readonly GUIContent configName = EditorGUIUtility.TrTextContent("配置名");
        public static readonly GUIContent mapSize = EditorGUIUtility.TrTextContent("地图大小");
        public static readonly GUIContent tileSize = EditorGUIUtility.TrTextContent("地块大小");
        public static readonly GUIContent layout = EditorGUIUtility.TrTextContent("布局方式");
        public static readonly GUIContent swizzle = EditorGUIUtility.TrTextContent("Swizzle");
        public static readonly GUIContent offset = EditorGUIUtility.TrTextContent("噪波偏移值");
        public static readonly GUIContent islandCount = EditorGUIUtility.TrTextContent("遍历孤岛次数");
        public static readonly GUIContent layer = EditorGUIUtility.TrTextContent("Layers");
        public static readonly GUIContent build = EditorGUIUtility.TrTextContent("构建");
        public static readonly GUIContent delete = EditorGUIUtility.TrTextContent("删除");
        public static readonly GUIContent name = EditorGUIUtility.TrTextContent("层级名称");
        public static readonly GUIContent sortLayer = EditorGUIUtility.TrTextContent("渲染顺序");
        public static readonly GUIContent animationFrameRate = EditorGUIUtility.TrTextContent("动画帧率");
        public static readonly GUIContent color = EditorGUIUtility.TrTextContent("颜色修正");
        public static readonly GUIContent anchor = EditorGUIUtility.TrTextContent("地块锚点");
        public static readonly GUIContent orientation = EditorGUIUtility.TrTextContent("方向");
        public static readonly GUIContent sortOrder = EditorGUIUtility.TrTextContent("渲染排序");
        public static readonly GUIContent mode = EditorGUIUtility.TrTextContent("模式");
        public static readonly GUIContent detectChunkCullingBounds = EditorGUIUtility.TrTextContent("检测块剔除边界方式");
        public static readonly GUIContent chunkCullingBounds = EditorGUIUtility.TrTextContent("块剔除边界");
        public static readonly GUIContent maskInteraction = EditorGUIUtility.TrTextContent("Mask Interaction");
        public static readonly GUIContent weight = EditorGUIUtility.TrTextContent("权重");

        public const string GUI_STYLE_TITLE_LABLE = "LargeBoldLabel"; // new GUIStyle("LargeBoldLabel");
        public const string GUI_STYLE_BOX_BACKGROUND = "OL box NoExpand"; // new GUIStyle("OL box NoExpand");
        public const string GUI_STYLE_LINE = "WhiteBackground";
        public const string GUI_STYLE_ADD_BUTTON = "OL Plus";
        public const string GUI_STYLE_MINUS = "OL Minus";

        public const string GUI_STYLE_FRAME_LINE = "OverrideMargin"; // new GUIStyle("OverrideMargin");
        public const string GUI_STYLE_LAYER_BACKGROUND = "MeTransitionSelect"; // new GUIStyle("MeTransitionSelect");
        public const string GUI_STYLE_LAYER_RANGE_SLIDER = "MeTransitionSelect"; // new GUIStyle("SelectionRect");
        public const string GUI_STYLE_LAYER_RANGE_SLIDER_SELECTION = "OL SelectedRow";

        public static Color inColor = new Color(1f, 0.92f, 0.01f, .8f);
        public static Color outColor = new Color(0, 0, 0, 0.2f);
    }

    public static class Extension
    {
        private static Color _color;

        public static void BeginColor(this EditorWindow window, Color color)
        {
            _color = GUI.color;
            GUI.color = color;
        }

        public static void EndColor(this EditorWindow window)
        {
            GUI.color = _color;
        }

        public static void BeginColor(this UnityEditor.Editor window, Color color)
        {
            _color = GUI.color;
            GUI.color = color;
        }
        
        public static void EndColor(this UnityEditor.Editor window)
        {
            GUI.color = _color;
        }

    }
}