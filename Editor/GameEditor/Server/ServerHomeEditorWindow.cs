using UnityEditor;
using UnityEngine;
using ZGame.Config;

namespace ZGame.Editor.Server
{
    public class ServerHomeEditorWindow : HomeEditorSceneWindow
    {
        public override string name => "Server";

        public override void OnDrawToolbar()
        {
            if (GUILayout.Button(EditorGUIUtility.IconContent(ZStyle.ADD_BUTTON_ICON), EditorStyles.toolbarButton))
            {
                string path = EditorUtility.OpenFolderPanel("Select Save Folder", Application.dataPath, "");
                if (string.IsNullOrEmpty(path))
                {
                    return;
                }

                GameServerOptions options = new GameServerOptions();
                AssetDatabase.CreateAsset(options, $"Assets/{path.Replace(Application.dataPath)}/new GameServerOptions.asset");
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                EditorWindow.GetWindow<GameEditorWindow>().OnEnable();
                EditorWindow.GetWindow<GameEditorWindow>().Repaint();
            }
        }
    }
}