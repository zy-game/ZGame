using System.Collections.Generic;

namespace ZEngine.Game
{
    /// <summary>
    /// 角色数据接口
    /// </summary>
    public interface IPlayerOptions : IOptions
    {
        string icon { get; set; }
        string prefab { get; set; }
        List<ISkillOptions> skills { get; }
    }
}