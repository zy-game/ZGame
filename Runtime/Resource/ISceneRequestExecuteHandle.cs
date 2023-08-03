namespace ZEngine.Resource
{
    /// <summary>
    /// 场景资源加载
    /// </summary>
    public interface ISceneRequestExecuteHandle : IExecuteHandle
    {
        void ObserverPorgress(ISubscribeExecuteHandle<float> subscribe);
    }
}