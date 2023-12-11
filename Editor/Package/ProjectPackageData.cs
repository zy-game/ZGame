using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;
using UnityEngine.Serialization;

namespace ZGame.Editor.Package
{
    public enum PackageState : byte
    {
        None,
        Update,
        Deletion
    }

    [Serializable]
    public class ProjectPackageData
    {
        public string name;
        public string version;
        public string url;

        [NonSerialized] public string latest;
        [NonSerialized] public PackageState state;
        [NonSerialized] public List<string> versions;
        [NonSerialized] public Dictionary<string, string> gitList;
    }
}