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

        public static UIOptions Default => new UIOptions(UILayer.Middle, SceneType.Overlap, CacheType.Permanent);

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

   
}