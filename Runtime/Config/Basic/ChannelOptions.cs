using System;
using UnityEngine;

namespace ZGame
{
    [Serializable]
    public class ChannelOptions
    {
        public string title;
        public string packageName;
        public Texture2D icon;
        public Sprite splash;
        public string appName;


        /// <summary>
        /// 启动参数
        /// </summary>
        public string args;
    }
}