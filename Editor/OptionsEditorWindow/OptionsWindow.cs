using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Editor.OptionsEditorWindow
{
    public class OptionsWindow : EditorWindow
    {
        [MenuItem("ZEngine/Options")]
        public static void OpenWindow()
        {
            GetWindow<OptionsWindow>(false, "Options", true);
        }

        private List<ScriptableObject> _objects;

        public void OnEnable()
        {
            
        }

        private void OnGUI()
        {
            
        }
    }
}