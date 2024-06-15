using System;
using UnityEditor;
using UnityEngine;
using ZGame.Config;

namespace ZGame.Editor.OSS
{
    public class OSSScriptableObjectEditorWindow : ScriptableObjectEditorWindow<ResourceServerOptions>
    {
        public override Type owner { get; } = typeof(OSSHomeEditorWindow);

        public OSSScriptableObjectEditorWindow(ResourceServerOptions data) : base(data)
        {
        }

        public override void OnDrawToolbar()
        {
            if (GUILayout.Button(EditorGUIUtility.FindTexture(ZStyle.DELETE_BUTTON_ICON), EditorStyles.toolbarButton))
            {
                this.OnDelete();
            }
        }

        public override void OnGUI()
        {
            this.target.type = (OSSType)EditorGUILayout.EnumPopup("服务器类型", this.target.type);
            this.target.bucket = EditorGUILayout.TextField("存储桶名称", this.target.bucket);
            this.target.region = EditorGUILayout.TextField("区域", this.target.region);
            this.target.key = EditorGUILayout.TextField("密匙ID", this.target.key);
            this.target.password = EditorGUILayout.TextField("密匙", this.target.password);
            this.target.enableAccelerate = EditorGUILayout.Toggle("是否启用加速", this.target.enableAccelerate);
        }
    }
}