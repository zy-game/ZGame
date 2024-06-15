using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using ZGame.Language;
using ZGame.Resource;

namespace ZGame.Config
{
    [HideInInspector]
    public class SubGameOptions : ScriptableObject
    {
        /// <summary>
        /// App 名称
        /// </summary>
        public string appName;

        /// <summary>
        /// 启动参数
        /// </summary>
        public string args;

        /// <summary>
        /// 版本号
        /// </summary>
        public string version;


        /// <summary>
        /// 主场景路径
        /// </summary>
        public string scenePath;

        /// <summary>
        /// 代码路径
        /// </summary>
        public string path;

        /// <summary>
        /// 包名
        /// </summary>
        public string packageName;


        /// <summary>
        /// 安装图标
        /// </summary>
        public Texture2D icon;

        /// <summary>
        /// 启动屏图片
        /// </summary>
        public Sprite splash;

        /// <summary>
        /// 语言配置
        /// </summary>
        public LanguageDefinition language;

        /// <summary>
        /// 代码模式
        /// </summary>
        public CodeMode mode;

        /// <summary>
        /// 默认资源模块
        /// </summary>
        public string mainPackageName;

        public string dllName => Path.GetFileNameWithoutExtension(path);
    }
}