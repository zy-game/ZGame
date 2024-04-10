// using UnityEditor;
// using UnityEngine;
// using ZGame.VFS;
//
// namespace ZGame.Editor.Inspector
// {
//     [CustomPropertyDrawer(typeof(OSSOptions))]
//     public class OSSOptionsProperty : PropertyDrawer
//     {
//         public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
//         {
//             EditorGUILayout.LabelField(property.FindPropertyRelative("title").stringValue);
//
//             // EditorGUI.PropertyField(position, property.FindPropertyRelative("title"), new GUIContent("资源服别称"), false);
//         }
//     }
//
//     public class SelectorAttribute : PropertyAttribute
//     {
//         public string label;
//     }
//
//     [CustomEditor(typeof(ResConfig))]
//     public class ResConfigInspectorEditor : UnityEditor.Editor
//     {
//         private bool vfsHeaderState = false;
//         private bool basicHeaderState = false;
//         private bool resServerHeaderState = false;
//
//
//         public override void OnInspectorGUI()
//         {
//             OnDrawingBasicSetting();
//             OnDrawingServerSetting();
//             OnDrawingVfsSetting();
//         }
//
//         private void OnDrawingBasicSetting()
//         {
//             basicHeaderState = EditorGUILayout.BeginFoldoutHeaderGroup(basicHeaderState, "基础设置");
//
//             EditorGUILayout.EndFoldoutHeaderGroup();
//         }
//
//         private void OnDrawingServerSetting()
//         {
//             // resServerHeaderState = EditorGUILayout.BeginFoldoutHeaderGroup(resServerHeaderState, "资源服务器设置");
//             EditorGUILayout.PropertyField(this.serializedObject.FindProperty("ossList"), new GUIContent("资源服列表"), true);
//             // EditorGUILayout.EndFoldoutHeaderGroup();
//         }
//
//         private void OnDrawingVfsSetting()
//         {
//             vfsHeaderState = EditorGUILayout.BeginFoldoutHeaderGroup(vfsHeaderState, "虚拟文件系统设置");
//             EditorGUILayout.EndFoldoutHeaderGroup();
//         }
//     }
// }