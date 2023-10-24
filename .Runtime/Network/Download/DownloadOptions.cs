using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ZEngine.Network
{
    public sealed class DownloadOptions
    {
        public string url;
        public object userData;
        public int version;
    }
}