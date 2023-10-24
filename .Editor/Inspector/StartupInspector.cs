using UnityEditor;

namespace Editor.Inspector
{
    [CustomEditor(typeof(Startup))]
    public class StartupInspector:UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            // serializedObject.FindProperty()
        }
    }
}