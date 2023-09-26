using System;
using System.Collections.Generic;

namespace ZEngine
{
    public interface ICommand : IDisposable
    {
        int id { get; }
    }

    public interface IBuilder : IDisposable
    {
        void Build();
    }

    /// <summary>
    /// 配置项
    /// </summary>
    public interface IOptions : IDisposable
    {
        int id { get; set; }
        string name { get; set; }
    }

    /// <summary>
    /// 角色数据接口
    /// </summary>
    public interface IPlayerOptions : IOptions
    {
        string icon { get; set; }
        string prefab { get; set; }
    }

    /// <summary>
    /// 技能数据
    /// </summary>
    public interface ISkillOptions : IOptions
    {
        string icon { get; set; }
        string prefab { get; set; }
    }

    /// <summary>
    /// 地图数据
    /// </summary>
    public interface IMapDataOptions : IOptions, IBuilder
    {
    }

    /// <summary>
    /// 场景数据
    /// </summary>
    public interface ISceneDataOptions : IOptions, IBuilder
    {
        IMapDataOptions mapData { get; set; }
    }
}