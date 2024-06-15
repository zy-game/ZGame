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
using UnityEditor;
using UnityEngine;

namespace ZGame.Editor.LinkerEditor
{
    [HideInInspector]
    public class LinkerConfig : ScriptableObject
    {
        public List<AssemblyLinker> assemblies;
    }

    [Serializable]
    public class LinkerDatable
    {
        public bool isOn;
        public string name;
    }

    [Serializable]
    public class AssemblyLinker
    {
        public string name;
        public List<LinkerDatable> classList;
    }
}