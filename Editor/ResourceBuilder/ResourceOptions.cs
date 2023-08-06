using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ZEngine.Editor.ResourceBuilder
{
    [ConfigOptions(ConfigOptions.Localtion.Project)]
    public class ResourceOptions : SingleScript<ResourceOptions>
    {
        [Header("模块列表")] public List<ResourceModuleManifest> modules;
    }

    [Serializable]
    public class ResourceModuleManifest
    {
        public string title;
        public Object folder;
        public VersionOptions version;


        public bool Search(string search)
        {
            if (title.Contains(search))
            {
                return true;
            }

            if (folder is null)
            {
                return false;
            }

            if (folder.name.Contains(search))
            {
                return true;
            }

            string path = AssetDatabase.GetAssetPath(folder);
            string[] files = Directory.GetFiles(path);
            return files.First(x => x.Contains(search) && x.EndsWith(".meta") is false) != null;
        }
    }
}