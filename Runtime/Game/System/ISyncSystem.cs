using TrueSync;

namespace ZGame.Game
{
    /// <summary>
    /// 同步系统
    /// </summary>
    public interface ISyncSystem : ISystem
    {
        void Sync(FrameData frameData);
    }

    public class SyncFrameSystem : ISyncSystem
    {
        public void Release()
        {
        }

        public uint priority { get; }

        public void Sync(FrameData frameData)
        {
            foreach (var VARIABLE in GameFrameworkEntry.ECS.AllOf<FrameSyncComponent>())
            {
                InputData input = frameData.GetFrameData(VARIABLE.id);
                if (input is null)
                {
                    GameFrameworkEntry.Logger.Log($"uid:{VARIABLE.id} 没有输入数据");
                    continue;
                }

                FP x = input.Get(0);
                FP y = input.Get(1);
                VARIABLE.rigidBody.velocity = new TSVector(x, 0, y);
            }
        }
    }
}