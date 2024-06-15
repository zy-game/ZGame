// using Sirenix.OdinInspector.Editor;
// using UnityEditor;
// using ZGame.UI;
//
// namespace ZGame.Editor.PSD2GUI
// {
//     [CustomEditor(typeof(LoopScrollCellView))]
//     public class LoopScrollCellViewEditor : OdinEditor
//     {
//         private LoopScrollCellView _target;
//
//         public void OnEnable()
//         {
//             _target = target as LoopScrollCellView;
//         }
//
//         public override void OnInspectorGUI()
//         {
//             EditorGUI.BeginChangeCheck();
//             this._target.height = EditorGUILayout.FloatField("高度", this._target.height);
//             if (EditorGUI.EndChangeCheck())
//             {
//                 EditorUtility.SetDirty(_target);
//                 AssetDatabase.SaveAssets();
//                 AssetDatabase.Refresh();
//             }
//
//             // UIBindInspector.OnDrawingInspectorGUI(_target, this);
//         }
//     }
// }