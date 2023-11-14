using UnityEditor;
using UnityEngine;
using ZGame.Editor.ResBuild.Config;

namespace ZGame.Editor.ResBuild
{
    public enum OSSType
    {
        Aliyun,
        Tencentyun,
    }

    [BindScene("版本管理", typeof(ResBuilder))]
    public class ResUploader : PageScene
    {
        
        public OSSType type;

        public override void OnEnable()
        {
            
        }

        public override void OnGUI()
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label("云存储");
            type = (OSSType)EditorGUILayout.EnumPopup(type, GUILayout.Width(100));
            
            
            GUILayout.EndHorizontal();
        }
    }

    public class OSSApi
    {
    }
}