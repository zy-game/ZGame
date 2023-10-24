using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace ZEngine.VFS
{
    [Config(Localtion.Internal)]
    public sealed class VFSOptions : ConfigScriptableObject<VFSOptions>
    {
        [Header("是否启用VFS")] public Switch vfsState;
        [Header("文件系统布局")] public VFSLayout layout;

        [Header("文件片段大小"), Range(1024, 1024 * 1024 * 64)]
        public int Lenght;

        [Header("最大片段数"), Range(1, 1024)] public int Count;

        [Header("文件缓存时长"), Range(60, 60 * 60)] public float time = 60;
    }
}