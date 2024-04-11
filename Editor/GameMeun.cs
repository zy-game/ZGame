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
using ZGame.Editor.ExcelExprot;
using ZGame.Editor.LinkerEditor;
using ZGame.VFS;
using OdinEditorIcons = Sirenix.Utilities.Editor.EditorIcons;

namespace ZGame.Editor
{
    public class GameMeun
    {
        [MenuItem("Tools/Clear PlayerPrefs")]
        public static void ClearPlayerPrefs()
        {
            PlayerPrefs.DeleteAll();
        }
    }
}