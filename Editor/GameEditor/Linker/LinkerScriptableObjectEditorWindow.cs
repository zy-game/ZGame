using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using HybridCLR.Editor;
using HybridCLR.Editor.Commands;
using HybridCLR.Editor.Link;
using HybridCLR.Editor.Meta;
using UnityEditor;
using UnityEngine;
using ZGame.Editor.LinkerEditor;

namespace ZGame.Editor.Linker
{
    public class LinkerScriptableObjectEditorWindow : ScriptableObjectEditorWindow<LinkerConfig>
    {
        private Dictionary<string, bool> foldoutList = new Dictionary<string, bool>();
        public override Type owner => typeof(LinkerHomeEditorWindow);

        public LinkerScriptableObjectEditorWindow(LinkerConfig data) : base(data)
        {
            foreach (var VARIABLE in data.assemblies)
            {
                foldoutList.Add(VARIABLE.name, false);
            }
        }


        public override void OnGUI()
        {
            for (int i = this.target.assemblies.Count - 1; i >= 0; i--)
            {
                EditorGUILayout.BeginHorizontal(ZStyle.BOX_BACKGROUND);
                foldoutList[this.target.assemblies[i].name] = EditorGUILayout.Foldout(foldoutList[this.target.assemblies[i].name], "");
                GUILayout.Space(-40);

                EditorGUILayout.LabelField(this.target.assemblies[i].name, EditorStyles.boldLabel);
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("全选", EditorStyles.toolbarButton))
                {
                    this.target.assemblies[i].classList.ForEach(x => x.isOn = true);
                }

                if (GUILayout.Button("反选", EditorStyles.toolbarButton))
                {
                    this.target.assemblies[i].classList.ForEach(x => x.isOn = false);
                }

                EditorGUILayout.EndHorizontal();

                if (foldoutList[this.target.assemblies[i].name])
                {
                    OnDrawAssemblyInfo(this.target.assemblies[i]);
                }

                GUILayout.Space(2);
            }
        }

        public override void OnDrawToolbar()
        {
            if (GUILayout.Button(EditorGUIUtility.FindTexture(ZStyle.REFRESH_BUTTON_ICON), EditorStyles.toolbarButton))
            {
                Refresh();
            }

            if (GUILayout.Button(EditorGUIUtility.FindTexture(ZStyle.PLAY_BUTTON_ICON), EditorStyles.toolbarButton))
            {
                Generic();
                foreach (var VARIABLE in this.target.assemblies)
                {
                    foldoutList.Add(VARIABLE.name, false);
                }
            }

            if (GUILayout.Button(EditorGUIUtility.FindTexture(ZStyle.DELETE_BUTTON_ICON), EditorStyles.toolbarButton))
            {
                this.OnDelete();
            }
        }

        private void OnDrawAssemblyInfo(AssemblyLinker linker)
        {
            if (linker.classList == null || linker.classList.Count == 0)
            {
                return;
            }

            foreach (var VARIABLE in linker.classList)
            {
                VARIABLE.isOn = GUILayout.Toggle(VARIABLE.isOn, VARIABLE.name);
            }
        }

        public void Refresh()
        {
            this.target.assemblies.Clear();
            Assembly[] libList = AppDomain.CurrentDomain.GetAssemblies();
            HashSet<string> libNames = new HashSet<string>();
            foreach (var assembly in libList)
            {
                string libName = assembly.GetName().Name;
                if (libName.StartsWith("UnityEditor") || libName.Contains(@"\UnityEditor") || libName.Contains(".Editor.") || libName.EndsWith("Editor"))
                {
                    continue;
                }

                if (this.target.assemblies.Exists(x => x.name == libName))
                {
                    continue;
                }

                AssemblyLinker assemblyLinker = new AssemblyLinker();
                assemblyLinker.name = libName;
                assemblyLinker.classList = new();
                foreach (var VARIABLE in assembly.GetTypes())
                {
                    if (libNames.Contains(VARIABLE.FullName) || VARIABLE.IsPublic is false || assemblyLinker.classList.Exists(x => x.name == VARIABLE.FullName))
                    {
                        continue;
                    }

                    assemblyLinker.classList.Add(new LinkerDatable() { name = VARIABLE.FullName, isOn = false });
                    libNames.Add(VARIABLE.FullName);
                }

                if (assemblyLinker.classList is not null && assemblyLinker.classList.Count > 0)
                {
                    this.target.assemblies.Add(assemblyLinker);
                    Debug.Log("add assembly : " + libName);
                }
            }
        }

        private Dictionary<string, List<string>> GetLinkerInfo()
        {
            EditorUtility.DisplayProgressBar("Generic Linker Vfs", "Complie DLL", 0);
            CompileDllCommand.CompileDllActiveBuildTarget();
            EditorUtility.DisplayProgressBar("Generic Linker Vfs", "Complie DLL", 1);
            List<string> hotfixAssemblies = SettingsUtil.HotUpdateAssemblyNamesExcludePreserved;
            Debug.Log(string.Join(",", hotfixAssemblies));
            var analyzer = new Analyzer(MetaUtil.CreateHotUpdateAndAOTAssemblyResolver(EditorUserBuildSettings.activeBuildTarget, hotfixAssemblies));
            var refTypes = analyzer.CollectRefs(hotfixAssemblies);
            var typesByAssembly = refTypes.GroupBy(t => t.DefinitionAssembly.Name.String).ToList();
            typesByAssembly.Sort((a, b) => String.Compare(a.Key, b.Key, StringComparison.Ordinal));

            EditorUtility.DisplayProgressBar("Generic Linker Vfs", "Write Linker Items", 0);
            Dictionary<string, List<string>> map = new Dictionary<string, List<string>>();
            typesByAssembly.ForEach(x =>
            {
                if (map.TryGetValue(x.Key, out List<string> groups) is false)
                {
                    map.Add(x.Key, groups = new List<string>());
                }

                groups.AddRange(x.Select(y => y.FullName));
            });

            foreach (var VARIABLE in this.target.assemblies)
            {
                if (map.TryGetValue(VARIABLE.name, out List<string> groups) is false)
                {
                    map.Add(VARIABLE.name, groups = new List<string>());
                }

                groups.AddRange(VARIABLE.classList.Where(x => x.isOn).Select(x => x.name));
            }

            return map;
        }

        public void Generic()
        {
            string path = $"{Application.dataPath}/link.xml";
            var map = GetLinkerInfo();
            var writer = System.Xml.XmlWriter.Create(path, new System.Xml.XmlWriterSettings { Encoding = Encoding.UTF8, Indent = true });
            writer.WriteStartDocument();
            writer.WriteStartElement("linker");
            foreach (var assembly in map)
            {
                if (assembly.Value is null || assembly.Value.Count == 0)
                {
                    continue;
                }

                writer.WriteStartElement("assembly");
                writer.WriteAttributeString("fullname", assembly.Key);
                foreach (var typeName in assembly.Value)
                {
                    writer.WriteStartElement("type");
                    writer.WriteAttributeString("fullname", typeName);
                    writer.WriteAttributeString("preserve", "all");
                    writer.WriteEndElement();
                }

                writer.WriteEndElement();
            }

            writer.WriteEndElement();
            writer.WriteEndDocument();
            writer.Close();
            EditorUtility.DisplayProgressBar("Generic Linker Vfs", "Write Linker Items", 1);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.ClearProgressBar();
            Debug.Log("link xml file generic completion. path:" + path);
        }
    }
}