namespace ZEngine.Game
{
    /// <summary>
    /// 场景数据
    /// </summary>
    public interface ISceneOptions : IOptions
    {
        string icon { get; set; }
        ISceneMapOptions mapData { get; set; }
    }

    /// <summary>
    /// 地图数据
    /// </summary>
    public interface ISceneMapOptions : IOptions
    {
        string icon { get; set; }
    }
}