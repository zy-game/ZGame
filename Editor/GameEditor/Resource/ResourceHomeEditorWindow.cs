using UnityEditor;
using UnityEngine;
using ZGame.Config;
using ZGame.Resource;

namespace ZGame.Editor.Resource
{
    public class ResourceHomeEditorWindow : HomeEditorSceneWindow
    {
        public override string name => "Resource";

        public override void OnDrawToolbar()
        {
            if (GUILayout.Button(EditorGUIUtility.IconContent(ZStyle.ADD_BUTTON_ICON), EditorStyles.toolbarButton))
            {
                string path = EditorUtility.OpenFolderPanel("Select Save Folder", Application.dataPath, "");
                if (string.IsNullOrEmpty(path))
                {
                    return;
                }

                ResourcePackageListManifest options = new ResourcePackageListManifest();
                AssetDatabase.CreateAsset(options, $"Assets/{path.Replace(Application.dataPath)}/new ResourcePackageListManifest.asset");
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                EditorWindow.GetWindow<GameEditorWindow>().OnEnable();
                EditorWindow.GetWindow<GameEditorWindow>().Repaint();
            }
        }
    }
}