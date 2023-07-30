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
            Assembly assembly = AppDomain.CurrentDomain.GetAssemblies().ToList().Find(x => x.GetName().Name == "ZEngine.Runtime");
            if (assembly is null)
            {
                return;
            }

            _objects = new List<ScriptableObject>();
            foreach (var type in assembly.GetTypes())
            {
                ConfigOptions options = type.GetCustomAttribute<ConfigOptions>();
                if (options is null)
                {
                    continue;
                }
                
            }
        }
    }
}