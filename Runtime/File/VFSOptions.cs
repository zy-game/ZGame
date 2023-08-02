using System;
using UnityEngine;

namespace ZEngine.VFS
{
    [ConfigOptions(ConfigOptions.Localtion.Internal)]
    public sealed class VFSOptions : SingleScript<VFSOptions>
    {
        [Header("是否启用VFS")] public Switch vfsState;
        [Header("文件扩展名")] public string extension;
        [Header("文件系统布局")] public VFSLayout layout;
        [Header("文件片段大小")] public int sgementLenght;
        [Header("最大片段数")] public int sgementCount;
        [Header("是否启用多线(WEBGL 不支持)")] public Switch mulitThreads;
        [Header("文件缓存时长")] public float time = 60;
    }
}