using System;
using System.Collections.Generic;
using Runtime.Core;
using UnityEngine;
using UnityEngine.Serialization;
using ZEngine.Core;
using ZEngine.VFS;
using ZEngine.Network;
using ZEngine.Options;
using ZEngine.Resource;

[ConfigOptions("", ConfigOptions.Localtion.Internal)]
public class AppConfig : SingleScriptObject<AppConfig>
{
    [SerializeField, Header("Reference Options")]
    public RefreshOptions refreshOptions;

    [SerializeField, Header("VFS Options")]
    public VFSOptions vfsOptions;

    [SerializeField, Header("Preload Options")]
    public ResourcePreloadOptions resourcePreloadOptions;

    [SerializeField, Header("Address Options")]
    public List<NetworkAddressOptions> AddressOptionsList;

    [SerializeField, Header("运行时缓存设置")] public CacheOptions cacheOptions;

    public static string GetLocalFilePath(string fileName)
    {
        return $"{Application.persistentDataPath}/{fileName}.{instance.vfsOptions.extension}";
    }
}