using System;
using UnityEngine;

namespace ZEngine.VFS
{
    [Serializable]
    public sealed class VFSOptions
    {
        [Header("是否启用VFS")] public Status vfsState;
        [Header("文件扩展名")] public string extension;
        [Header("文件系统布局")] public VFSLayout layout;
        [Header("文件片段大小")] public int sgementLenght;
        [Header("最大片段数")] public int sgementCount;
    }
}