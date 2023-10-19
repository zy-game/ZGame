using System.Collections.Generic;

namespace ZEngine.Game
{
    /// <summary>
    /// 资源对象配置
    /// </summary>
    public interface IObjectOptions : IOptions
    {
        string icon { get; set; }
        string prefab { get; set; }
    }

    /// <summary>
    /// 角色数据接口
    /// </summary>
    public interface IPlayerOptions : IObjectOptions
    {
        List<ISkillOptions> skills { get; }
    }

    /// <summary>
    /// 道具配置
    /// </summary>
    public interface IPropOptions : IObjectOptions
    {
        int propType { get; }
    }

    /// <summary>
    /// 技能数据
    /// </summary>
    public interface ISkillOptions : IObjectOptions
    {
        float cd { get; set; }
        float usege { get; set; }
        ushort level { get; set; }
        List<ISkillEventLayerOptions> layers { get; set; }
    }

    public enum SkillEventLayerType : byte
    {
    }

    public interface ISkillEventLayerOptions : IOptions
    {
        int startFrame { get; }
        int endFrame { get; }
        SkillEventLayerType layerType { get; }
    }

    /// <summary>
    /// 地图数据
    /// </summary>
    public interface IMapDataOptions : IObjectOptions
    {
    }

    /// <summary>
    /// 场景数据
    /// </summary>
    public interface ISceneDataOptions : IObjectOptions
    {
        IMapDataOptions mapData { get; set; }
    }
}