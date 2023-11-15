using UnityEditor;
using ZGame.Editor.ResBuild.Config;

namespace ZGame.Editor.ResBuild
{
    [BindScene("设置", typeof(ResBuilder))]
    public class OSSSetting : PageScene
    {
        private SerializedProperty property;

        private SerializedObject serializedObject;

        public override void OnEnable()
        {
            serializedObject = new SerializedObject(BuilderConfig.instance);
            property = serializedObject.FindProperty("ossList");
        }

        public override void OnGUI()
        {
            if (property == null)
            {
                return;
            }

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(property);
            if (EditorGUI.EndChangeCheck() || serializedObject.ApplyModifiedProperties())
            {
                BuilderConfig.Saved();
            }
        }
    }
}