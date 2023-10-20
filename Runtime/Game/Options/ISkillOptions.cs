using System.Collections.Generic;

namespace ZEngine.Game
{
    /// <summary>
    /// 技能数据
    /// </summary>
    public interface ISkillOptions : IOptions
    {
        float cd { get; set; }
        float usege { get; set; }
        ushort level { get; set; }
        string icon { get; set; }
        string prefab { get; set; }
        List<ISkillEventOptions> layers { get; set; }
    }
    
    public enum SkillEventType : byte
    {
    }

    public interface ISkillEventOptions : IOptions
    {
        int startFrame { get; }
        int endFrame { get; }
        SkillEventType eventType { get; }
    }
}