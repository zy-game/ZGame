using System;
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
using UnityEditor;
using UnityEngine;

namespace ZGame.Editor.LinkerEditor
{
    [ResourceReference("Assets/Settings/LinkerConfig.asset")]
    public class LinkerConfig : SingletonScriptableObject<LinkerConfig>
    {
        public List<Linker> assemblies;

        public override void OnAwake()
        {
            if (assemblies is null)
            {
                assemblies = new List<Linker>();
            }

            Debug.Log("assembly count:" + assemblies.Count);
            if (assemblies.Count == 0)
            {
                Refresh();
            }
        }

        public void Refresh()
        {
            assemblies.Clear();
            Assembly[] libList = AppDomain.CurrentDomain.GetAssemblies();
            HashSet<string> libNames = new HashSet<string>();
            foreach (var VARIABLE in libList)
            {
                string libName = VARIABLE.GetName().Name;
                if (libName.StartsWith("UnityEditor") || libName.Contains(@"\UnityEditor") || libName.Contains(".Editor.") || libName.EndsWith("Editor"))
                {
                    continue;
                }

                if (assemblies.Exists(x => x.name == libName))
                {
                    continue;
                }

                Linker linker = new Linker();
                linker.name = libName;
                Type[] types = VARIABLE.GetTypes();
                linker.nameSpaces = new List<LinkNameSpace>();
                for (int i = 0; i < types.Length; i++)
                {
                    if (libNames.Contains(types[i].FullName) || types[i].IsPublic is false)
                    {
                        continue;
                    }

                    LinkNameSpace nameSpace = linker.nameSpaces.Find(x => x.name == types[i].Namespace);
                    if (nameSpace == null)
                    {
                        linker.nameSpaces.Add(nameSpace = new LinkNameSpace()
                        {
                            name = types[i].Namespace,
                            classes = new List<LinkClass>(),
                        });
                    }

                    if (nameSpace.classes is null)
                    {
                        nameSpace.classes = new List<LinkClass>();
                    }

                    nameSpace.classes.Add(new LinkClass()
                    {
                        isOn = false,
                        name = types[i].Name,
                        nameSpace = types[i].Namespace
                    });
                    libNames.Add(types[i].FullName);
                }

                if (linker.nameSpaces is not null && linker.nameSpaces.Count > 0)
                {
                    assemblies.Add(linker);
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

                VARIABLE.nameSpaces.ForEach(x => x.classes.ForEach(y =>
                {
                    if (y.isOn is false)
                    {
                        return;
                    }

                    if (groups.Contains(y.FullName) is false)
                    {
                        groups.Add(y.FullName);
                    }
                }));
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
    public class Linker
    {
        public string name;
        public List<LinkNameSpace> nameSpaces;
        [NonSerialized] public bool enable;

        public bool All
        {
            get { return nameSpaces.All(x => x.All); }
        }

        public bool IsSelection
        {
            get { return nameSpaces.First(x => x.IsSelection) != null; }
        }


        public void SelectAll()
        {
            nameSpaces.ForEach(x => x.SelectAll());
        }

        public void UnselectAll()
        {
            nameSpaces.ForEach(x => x.UnselectAll());
        }
    }

    [Serializable]
    public class LinkNameSpace
    {
        public string name;
        public List<LinkClass> classes;

        [NonSerialized] public bool enable;

        public bool All
        {
            get { return classes.All(x => x.isOn); }
        }

        public bool IsSelection
        {
            get { return classes.First(x => x.isOn) != null; }
        }

        public void SelectAll()
        {
            classes.ForEach(x => x.isOn = true);
        }

        public void UnselectAll()
        {
            classes.ForEach(x => x.isOn = false);
        }
    }

    [Serializable]
    public class LinkClass
    {
        public bool isOn;
        public string name;
        public string nameSpace;

        public string FullName
        {
            get { return nameSpace + "." + name; }
        }
    }
}