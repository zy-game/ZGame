using UnityEditor;
using ZGame.Editor.ResBuild.Config;

namespace ZGame.Editor.ResBuild
{
    [Options(typeof(UploadSeting))]
    [BindScene("设置", typeof(ResBuilder))]
    public class OSSSetting : PageScene
    {
        private SerializedProperty property;

        public override void OnEnable()
        {
            property = new SerializedObject(BuilderConfig.instance.uploadSeting).FindProperty("optionsList");
        }

        public override void OnGUI()
        {
            if (property == null)
            {
                return;
            }

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(property);
            if (EditorGUI.EndChangeCheck())
            {
                BuilderConfig.Saved();
            }
        }
    }
}