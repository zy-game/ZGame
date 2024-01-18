using UnityEditor;

namespace ZGame.Editor
{
    [SubPageSetting("VFS Setting", typeof(GlobalWindow))]
    public class VFSWindow : SubPage
    {
        private bool show3;

        public override void OnGUI()
        {
            EditorGUI.BeginChangeCheck();
            BasicConfig.instance.vfsConfig.chunkSize = EditorGUILayout.IntField("并发运行数量", BasicConfig.instance.vfsConfig.chunkSize);
            BasicConfig.instance.vfsConfig.chunkCount = EditorGUILayout.IntField("并发运行数量", BasicConfig.instance.vfsConfig.chunkCount);
            if (EditorGUI.EndChangeCheck())
            {
                BasicConfig.OnSave();
            }
        }
    }
}