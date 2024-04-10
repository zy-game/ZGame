using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Serialization;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;
using ZGame.Config;
using OdinEditorIcons = Sirenix.Utilities.Editor.EditorIcons;

namespace ZGame.Editor
{
    class BuilderPackageWindow : OdinEditorWindow
    {
        [Button("BuiBui")]
        public void Builder()
        {
        }
    }

    public class GameMeun : OdinMenuEditorWindow
    {
        [MenuItem("Custom Editor/Main Editor")]
        private static void OpenWindow()
        {
            var window = GetWindow<GameMeun>();
            window.position = GUIHelper.GetEditorWindowRect().AlignCenter(800, 600);
        }


        protected override OdinMenuTree BuildMenuTree()
        {
            OdinMenuTree tree = new OdinMenuTree(supportsMultiSelect: true);
            tree.Add("Home", this, OdinEditorIcons.House);
            tree.Add("Player Settings", Resources.FindObjectsOfTypeAll<PlayerSettings>().FirstOrDefault());
            tree.Add("Builder", null, OdinEditorIcons.UnityGameObjectIcon);
            tree.EnumerateTree(x => { });
            return tree;
        }

        protected override IEnumerable<object> GetTargets()
        {
            foreach (var VARIABLE in this.MenuTree.Selection)
            {
                if (VARIABLE.Name == "Builder")
                {
                    yield return BuilderConfig.instance;
                    yield return new BuilderPackageWindow();
                }
            }
        }
    }
}