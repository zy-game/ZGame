using UnityEditor;
using UnityEngine;
using ZGame.Config;
using ZGame.Resource;

namespace ZGame.Editor.OSS
{
    public class OSSHomeEditorWindow : HomeEditorSceneWindow
    {
        public override string name => "OSS";


        public override void OnDrawToolbar()
        {
            if (GUILayout.Button(EditorGUIUtility.IconContent(ZStyle.ADD_BUTTON_ICON), EditorStyles.toolbarButton))
            {
                string path = EditorUtility.OpenFolderPanel("Select Save Folder", Application.dataPath, "");
                if (string.IsNullOrEmpty(path))
                {
                    return;
                }

                ResourceServerOptions options = new ResourceServerOptions();
                AssetDatabase.CreateAsset(options, $"Assets/{path.Replace(Application.dataPath)}/new ResourceServerOptions.asset");
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                EditorWindow.GetWindow<GameEditorWindow>().OnEnable();
                EditorWindow.GetWindow<GameEditorWindow>().Repaint();
            }
        }
    }
}