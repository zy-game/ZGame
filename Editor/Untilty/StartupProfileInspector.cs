using UnityEditor;
using UnityEngine;

namespace ZGame.Editor
{
    [CustomEditor(typeof(GameFrameworkStartup))]
    public class StartupProfileInspector : GameInspectorEditorWindow
    {
        public override void OnInspectorGUI()
        {
            if (Application.isPlaying is false)
            {
                EditorGUILayout.LabelField("Waiting Playing...", EditorStyles.boldLabel);
                return;
            }

            GameFrameworkEntry.GameFrameworkContent.OnProfile();
        }
    }
}