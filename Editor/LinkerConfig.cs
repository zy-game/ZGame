using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using dnlib.DotNet;
using HybridCLR.Editor;
using HybridCLR.Editor.Commands;
using HybridCLR.Editor.Link;
using HybridCLR.Editor.Meta;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace ZGame.Editor.LinkerEditor
{
    public class ListSelector : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
        }
    }

    [RefPath("Assets/Settings/LinkerConfig.asset")]
    public class LinkerConfig : BaseConfig<LinkerConfig>
    {
        [Title("Link 设置"), HideLabel, TabGroup("assembly")]
        public List<AssemblyLinker> assemblies;


        [Button("Refresh")]
        public void Refresh()
        {
            assemblies.Clear();
            Assembly[] libList = AppDomain.CurrentDomain.GetAssemblies();
            HashSet<string> libNames = new HashSet<string>();
            foreach (var assembly in libList)
            {
                string libName = assembly.GetName().Name;
                if (libName.StartsWith("UnityEditor") || libName.Contains(@"\UnityEditor") || libName.Contains(".Editor.") || libName.EndsWith("Editor"))
                {
                    continue;
                }

                if (assemblies.Exists(x => x.name == libName))
                {
                    continue;
                }

                AssemblyLinker assemblyLinker = new AssemblyLinker();
                assemblyLinker.name = libName;
                assemblyLinker.classs = new();
                foreach (var VARIABLE in assembly.GetTypes())
                {
                    if (libNames.Contains(VARIABLE.FullName) || VARIABLE.IsPublic is false || assemblyLinker.classs.Contains(VARIABLE.FullName))
                    {
                        continue;
                    }

                    assemblyLinker.classs.Add(VARIABLE.FullName);
                    libNames.Add(VARIABLE.FullName);
                }

                if (assemblyLinker.classs is not null && assemblyLinker.classs.Count > 0)
                {
                    assemblies.Add(assemblyLinker);
                    Debug.Log("add assembly : " + libName);
                }
            }
        }

        private Dictionary<string, List<string>> GetLinkerInfo()
        {
            EditorUtility.DisplayProgressBar("Generic Linker File", "Complie DLL", 0);
            CompileDllCommand.CompileDllActiveBuildTarget();
            EditorUtility.DisplayProgressBar("Generic Linker File", "Complie DLL", 1);
            List<string> hotfixAssemblies = SettingsUtil.HotUpdateAssemblyNamesExcludePreserved;
            Debug.Log(string.Join(",", hotfixAssemblies));
            var analyzer = new Analyzer(MetaUtil.CreateHotUpdateAndAOTAssemblyResolver(EditorUserBuildSettings.activeBuildTarget, hotfixAssemblies));
            var refTypes = analyzer.CollectRefs(hotfixAssemblies);
            var typesByAssembly = refTypes.GroupBy(t => t.DefinitionAssembly.Name.String).ToList();
            typesByAssembly.Sort((a, b) => String.Compare(a.Key, b.Key, StringComparison.Ordinal));

            EditorUtility.DisplayProgressBar("Generic Linker File", "Write Linker Items", 0);
            Dictionary<string, List<string>> map = new Dictionary<string, List<string>>();
            typesByAssembly.ForEach(x =>
            {
                if (map.TryGetValue(x.Key, out List<string> groups) is false)
                {
                    map.Add(x.Key, groups = new List<string>());
                }

                groups.AddRange(x.Select(y => y.FullName));
            });

            foreach (var VARIABLE in assemblies)
            {
                if (map.TryGetValue(VARIABLE.name, out List<string> groups) is false)
                {
                    map.Add(VARIABLE.name, groups = new List<string>());
                }

                foreach (var classs in VARIABLE.selection)
                {
                    if (groups.Contains(classs) is false)
                    {
                        groups.Add(classs);
                    }
                }
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
            EditorUtility.DisplayProgressBar("Generic Linker File", "Write Linker Items", 1);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.ClearProgressBar();
            Debug.Log("link xml file generic completion. path:" + path);
        }
    }

    [Serializable]
    public class AssemblyLinker
    {
        [HideLabel, HorizontalGroup("$name")] public string name;

        [HideInInspector] public List<string> classs;

        [ValueDropdown("classs", ExcludeExistingValuesInList = true), HorizontalGroup("$name")]
        public List<string> selection;

        [Button("全选"), HorizontalGroup("$name", 60)]
        void SelectionAll()
        {
            selection.Clear();
            selection.AddRange(classs);
        }

        [Button("反选"), HorizontalGroup("$name", 60)]
        void ReverseSelection()
        {
            foreach (var VARIABLE in classs)
            {
                if (selection.Contains(VARIABLE))
                {
                    selection.Remove(VARIABLE);
                }
                else
                {
                    selection.Add(VARIABLE);
                }
            }
        }

        IEnumerable GetAllNameSpace()
        {
            return classs;
        }
    }
}