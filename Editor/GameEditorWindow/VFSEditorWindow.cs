using UnityEditor;
using UnityEngine;
using ZGame.VFS;

namespace ZGame.Editor
{
    [GameSubEditorWindowOptions("虚拟文件系统", typeof(RuntimeEditorWindow))]
    public class VFSEditorWindow : GameSubEditorWindow
    {
        private bool show3;

        public override void OnGUI()
        {
            VFSConfig.instance.enable = EditorGUILayout.Toggle("是否开启虚拟文件系统", VFSConfig.instance.enable);
            VFSConfig.instance.chunkSize = EditorGUILayout.IntField("分片大小", VFSConfig.instance.chunkSize);
            VFSConfig.instance.chunkCount = EditorGUILayout.IntField("单个文件最大分片数", VFSConfig.instance.chunkCount);
            if (Event.current.type == EventType.KeyDown && Event.current.control && Event.current.keyCode == KeyCode.S)
            {
                VFSConfig.Save();
            }
        }

        public override void SaveChanges()
        {
            VFSConfig.Save();
        }
    }
}