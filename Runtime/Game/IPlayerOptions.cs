using System.Collections.Generic;

namespace ZEngine.Game
{
    public interface IGameObjectOptions : IOptions
    {
        string icon { get; set; }
        string prefab { get; set; }
    }

    /// <summary>
    /// 角色数据接口
    /// </summary>
    public interface IPlayerOptions : IGameObjectOptions
    {
        List<ISkillOptions> skills { get; }
    }

    /// <summary>
    /// 道具配置
    /// </summary>
    public interface IPropOptions : IGameObjectOptions
    {
        int propType { get; }
    }

    /// <summary>
    /// 技能数据
    /// </summary>
    public interface ISkillOptions : IGameObjectOptions
    {
        float cd { get; set; }
        float usege { get; set; }
        ushort level { get; set; }
    }

    /// <summary>
    /// 地图数据
    /// </summary>
    public interface IMapDataOptions : IGameObjectOptions
    {
    }

    /// <summary>
    /// 场景数据
    /// </summary>
    public interface ISceneDataOptions : IGameObjectOptions
    {
        IMapDataOptions mapData { get; set; }
    }
}