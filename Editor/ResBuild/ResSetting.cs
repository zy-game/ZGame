using UnityEditor;
using UnityEngine;
using ZGame.Editor.ResBuild.Config;

namespace ZGame.Editor.ResBuild
{
    [BindScene("设置", typeof(ResBuilder))]
    public class ResSetting : PageScene
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
            GUILayout.BeginVertical("Output Seting", EditorStyles.helpBox);
            GUILayout.Space(20);
            BuilderConfig.instance.comperss = (BuildAssetBundleOptions)EditorGUILayout.EnumPopup("压缩方式", BuilderConfig.instance.comperss);
            BuilderConfig.instance.useActiveTarget = EditorGUILayout.Toggle("是否跟随激活平台", BuilderConfig.instance.useActiveTarget);
            EditorGUI.BeginDisabledGroup(BuilderConfig.instance.useActiveTarget);
            BuilderConfig.instance.target = (BuildTarget)EditorGUILayout.EnumPopup("编译平台", BuilderConfig.instance.target);
            EditorGUI.EndDisabledGroup();
            BuilderConfig.instance.output = EditorGUILayout.TextField("输出路径", BuilderConfig.instance.output);
            BuilderConfig.instance.fileExtension = EditorGUILayout.TextField("文件扩展名", BuilderConfig.instance.fileExtension);
            GUILayout.EndVertical();
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