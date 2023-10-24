using UnityEditor;
using ZEngine.Utility;

namespace Editor.Inspector
{
    [CustomEditor(typeof(AutoSize))]
    public class AutoSizeInspector : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();
            base.OnInspectorGUI();
            AutoSize a = (AutoSize)this.target;
            if (EditorGUI.EndChangeCheck())
            {
                a.Refersh();
            }
            // EditorGUILayout.PropertyField(serializedObject.FindProperty("horizontalFit"));
            // EditorGUILayout.PropertyField(serializedObject.FindProperty("horizontalFit"));
            // EditorGUILayout.PropertyField(serializedObject.FindProperty("target"));
        }
    }
}