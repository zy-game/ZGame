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

        public static T Requery<T>() where T : IOptions
        {
            return default;
        }
    }
}