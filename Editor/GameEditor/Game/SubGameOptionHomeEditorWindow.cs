using UnityEditor;
using UnityEngine;
using ZGame.Config;
using ZGame.Editor.LinkerEditor;

namespace ZGame.Editor
{
    public class SubGameOptionHomeEditorWindow : HomeEditorSceneWindow
    {
        public override string name => "Game";


        public override void OnDrawToolbar()
        {
            if (GUILayout.Button(EditorGUIUtility.IconContent(ZStyle.ADD_BUTTON_ICON), EditorStyles.toolbarButton))
            {
                string path = EditorUtility.OpenFolderPanel("Select Save Folder", Application.dataPath, "");
                if (string.IsNullOrEmpty(path))
                {
                    return;
                }

                SubGameOptions options = new SubGameOptions();
                AssetDatabase.CreateAsset(options, $"Assets/{path.Replace(Application.dataPath)}/new SubGameOptions.asset");
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                EditorWindow.GetWindow<GameEditorWindow>().OnEnable();
                EditorWindow.GetWindow<GameEditorWindow>().Repaint();
            }
        }
    }
}