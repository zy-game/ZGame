using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using ZEngine.Core;
using ZEngine.File;
using ZEngine.Network;
using ZEngine.Options;
using ZEngine.Resource;

[ConfigOptions("", ConfigOptions.Localtion.Internal)]
public class AppConfig : SingleScriptObject<AppConfig>
{
    [SerializeField, Header("Reference Options")]
    public RefreshOptions refreshOptions;

    [SerializeField, Header("File System Options")]
    public FileSystemOptions fileSystemOptions;

    [FormerlySerializedAs("preloadOptions")] [SerializeField, Header("Preload Options")]
    public ResourcePreloadOptions resourcePreloadOptions;

    [SerializeField, Header("Address Options")]
    public List<NetworkAddressOptions> AddressOptionsList;
}