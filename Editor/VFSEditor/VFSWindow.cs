using UnityEditor;
using UnityEngine;

namespace ZGame.Editor
{
    [PageConfig("VFS", typeof(RuntimeEditorWindow))]
    public class VFSWindow : ToolbarScene
    {
        private bool show3;

        public override void OnGUI()
        {
            BasicConfig.instance.vfsConfig.chunkSize = EditorGUILayout.IntField("分片大小", BasicConfig.instance.vfsConfig.chunkSize);
            BasicConfig.instance.vfsConfig.chunkCount = EditorGUILayout.IntField("单个文件最大分片数", BasicConfig.instance.vfsConfig.chunkCount);
            if (Event.current.type == EventType.KeyDown && Event.current.control && Event.current.keyCode == KeyCode.S)
            {
                BasicConfig.OnSave();
            }
        }
    }
}