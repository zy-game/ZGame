using System;
using UnityEngine.Serialization;

namespace ZGame.UI
{
    /// <summary>
    /// UI选项
    /// </summary>
    public sealed class UIOptions : Attribute
    {
        public int layer;
        public SceneType sceneType;
        public CacheType cacheType;

        public UIOptions(int layer, SceneType sceneType, CacheType cacheType)
        {
            this.layer = layer;
            this.sceneType = sceneType;
            this.cacheType = cacheType;
        }

        public UIOptions(UILayer layer, SceneType sceneType, CacheType cacheType) : this((int)layer, sceneType, cacheType)
        {
        }
    }

    /// <summary>
    /// 界面默认层级
    /// </summary>
    public enum UILayer : byte
    {
        /// <summary>
        /// 底层
        /// </summary>
        Background = 1,

        /// <summary>
        /// 内容层
        /// </summary>
        Middle = 50,

        /// <summary>
        /// 弹窗层
        /// </summary>
        Popup = 100,

        /// <summary>
        /// 通知弹窗层
        /// </summary>
        Notification = 150,
    }

    /// <summary>
    /// 界面显示方式
    /// </summary>
    public enum SceneType : byte
    {
        /// <summary>
        /// 叠加窗口
        /// </summary>
        Addition,

        /// <summary>
        /// 覆盖窗口
        /// </summary>
        Overlap,
    }

    /// <summary>
    /// 缓存类型
    /// </summary>
    public enum CacheType : byte
    {
        /// <summary>
        /// 零时缓存，用完就删，不会缓存此标记的物体
        /// </summary>
        Temp,

        /// <summary>
        /// 常驻行，用完回收进缓存池中，等待下次使用
        /// </summary>
        Permanent,

        /// <summary>
        /// 自动管理，由框架根据运行时状态自动管理卸载
        /// </summary>
        Auto,
    }
}