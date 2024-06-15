using System;
using UnityEditor;
using UnityEngine;
using ZGame.Config;

namespace ZGame.Editor.Server
{
    public class ServerScriptableObjectEditorWindow : ScriptableObjectEditorWindow<GameServerOptions>
    {
        public override Type owner => typeof(ServerHomeEditorWindow);

        public ServerScriptableObjectEditorWindow(GameServerOptions data) : base(data)
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
            this.target.hosting = EditorGUILayout.TextField(new GUIContent("Hosting"), this.target.hosting);
            this.target.port = (ushort)Math.Clamp(EditorGUILayout.IntField(new GUIContent("Port"), this.target.port), ushort.MinValue, ushort.MaxValue);
        }
    }
}