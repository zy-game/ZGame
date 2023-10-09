using System;
using System.Collections.Generic;

namespace ZEngine
{
    public interface ICommand : IDisposable
    {
        int id { get; }
    }


    /// <summary>
    /// 配置项
    /// </summary>
    public interface IOptions : IDisposable
    {
        int id { get; set; }
        string name { get; set; }
        string icon { get; set; }
        string prefab { get; set; }
        string describe { get; set; }
    }

    /// <summary>
    /// 角色数据接口
    /// </summary>
    public interface IPlayerOptions : IOptions
    {
        List<ISkillOptions> skills { get; }
    }

    /// <summary>
    /// 技能数据
    /// </summary>
    public interface ISkillOptions : IOptions
    {
        float cd { get; set; }
        float usege { get; set; }
        ushort level { get; set; }
    }

    /// <summary>
    /// 地图数据
    /// </summary>
    public interface IMapDataOptions : IOptions
    {
    }

    /// <summary>
    /// 场景数据
    /// </summary>
    public interface ISceneDataOptions : IOptions
    {
        IMapDataOptions mapData { get; set; }
    }

    public class OptionsName : Attribute
    {
        public string name;

        public OptionsName(string name)
        {
            this.name = name;
        }
    }
}