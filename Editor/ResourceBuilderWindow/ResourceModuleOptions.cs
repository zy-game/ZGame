using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using ZEngine.Resource;
using Object = UnityEngine.Object;

namespace ZEngine.Editor.ResourceBuilder
{
    public enum OSSService : byte
    {
        OSS,
        COS
    }

    [Config(Localtion.Project)]
    public class ResourceModuleOptions : SingleScript<ResourceModuleOptions>
    {
        [Header("云存储")] public List<OSSOptions> options;
        [Header("模块列表")] public List<ResourceModuleManifest> modules;
    }

    [Serializable]
    public class OSSOptions
    {
        [Header("是否启用")] public Switch isOn;
        [Header("存储服务商")] public OSSService service;
        [Header("Bucket")] public string bucket;
        [Header("密钥ID")] public string keyID;
        [Header("密钥")] public string key;
        [Header("节点地址")] public string url;
    }

    [Serializable]
    public class ResourceModuleManifest
    {
        public string title;
        public VersionOptions version;
        public List<ResourceBundleManifest> bundles;

        public bool Search(string search)
        {
            if (title.IsNullOrEmpty() is false && title.Contains(search))
            {
                return true;
            }

            if (bundles is null || bundles.Count is 0)
            {
                return false;
            }

            return bundles.Find(x => x.Search(search)) != null;
        }
    }

    [Serializable]
    public class ResourceBundleManifest
    {
        public string name;
        public Object folder;
        public VersionOptions version;

        [NonSerialized] public List<Object> files;
        [NonSerialized] public bool isOn;
        [NonSerialized] public bool foldout;

        public bool Search(string search)
        {
            if (name.IsNullOrEmpty() is false && name.Contains(search))
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

            if (files is null || files.Count is 0)
            {
                return false;
            }

            return files.Find(x => x.name.Contains(search));
        }
    }
}