using System.Threading.Tasks;
using HybridCLR;
using UnityEngine.UI;
using UnityEngine.Video;
using ZEngine;
using ZEngine.Resource;
using Object = UnityEngine.Object;

namespace ZEngine
{
    public static class SystemExtension
    {
        public static bool IsNullOrEmpty(this string value)
        {
            return string.IsNullOrEmpty(value);
        }
    }
}