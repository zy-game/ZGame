using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;
using ZGame.Editor.Command;
using ZGame.Editor.LinkerEditor;

namespace ZGame.Editor.Inspector
{
    [CustomEditor(typeof(LinkerConfig))]
    public class LinkerInspectorWindow : OdinEditor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (GUILayout.Button("Generic"))
            {
                LinkerConfig.instance.Generic();
            }
        }
    }
}