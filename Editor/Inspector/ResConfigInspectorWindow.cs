using Sirenix.OdinInspector.Editor;
using UnityEditor;
using ZGame.VFS;

namespace ZGame.Editor.Inspector
{
    [CustomEditor(typeof(ResConfig))]
    public class ResConfigInspectorWindow : OdinEditor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
        }
    }
}