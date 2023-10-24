namespace ZGame
{
    /// <summary>
    /// 配置项
    /// </summary>
    public interface IOptions : IEntity
    {
        string name { get; }
        uint version { get; }
        void Active();
        void Inactive();
    }
}