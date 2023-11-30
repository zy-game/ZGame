using UnityEngine;

namespace ZGame.Config
{
    [CreateAssetMenu(menuName = "ZGame/VFS Setting", fileName = "VFSSetting", order = 1)]
    public class VFSSetting : ScriptableObject
    {
        [Header("是否启用虚拟文件系统")] public bool enable = true;
        [Header("虚拟文件分块大小")] public int chunkSize = 1024 * 1024;
        [Header("虚拟文件分块数量")] public int chunkCount = 1024;
    }
}