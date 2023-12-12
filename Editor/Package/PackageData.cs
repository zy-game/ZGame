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
    }

    public enum InstallState
    {
        None,
        Install,
        Uninstall,
        Installeding
    }


    [Serializable]
    public class PackageData
    {
        public string title;
        public string name;
        public string version;
        public string url;
        public InstallState installed;
        public PackageState state;

        public string recommended;

        [NonSerialized] public List<string> versions;
        [NonSerialized] public int start = 0;
        [NonSerialized] public int end = 11;
        [NonSerialized] public int cur = 0;
        [NonSerialized] public string icon = "";
        
        public static PackageData OnCreate(PackageInfo info)
        {
            PackageData packageData = new PackageData();
            packageData.name = info.name;
            packageData.version = info.version;
            packageData.title = info.displayName;
            packageData.installed = InstallState.Install;
            packageData.recommended = info.versions.recommended;
            string[] split = info.packageId.Split("@");
            packageData.url = split[1].StartsWith("https") ? split[1] : split[0];
            return packageData;
        }
    }
}