using System;
using UnityEngine;
using ZGame.Config;

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
        public string language;

        /// <summary>
        /// 启动参数
        /// </summary>
        public string args;
    }
}