namespace ZGame.Game
{
    /// <summary>
    /// 同步系统
    /// </summary>
    public interface ISyncSystem : ISystem
    {
        void Sync(SyncData syncData);
    }
    
    // public class SyncFrameSystem : ISyncSystem
    // {
    //     public void Release()
    //     {
    //     }
    //
    //     public uint priority { get; }
    //
    //     public void Sync(SyncData syncData)
    //     {
    //         foreach (var VARIABLE in GameFrameworkEntry.ECS.AllOf<FrameSyncComponent>())
    //         {
    //             InputData input = syncData.GetFrameData(VARIABLE.id);
    //             FP x = input.Get(0);
    //             FP y = input.Get(1);
    //             TSVector vector = new TSVector(x, 0, y);
    //             VARIABLE.transform.Translate(vector * Simulator.DeltaTime);
    //             TSQuaternion targetRotation = TSQuaternion.LookRotation(vector.normalized, TSVector.up);
    //             VARIABLE.transform.rotation = TSQuaternion.Slerp(VARIABLE.transform.rotation, targetRotation, Simulator.DeltaTime * 5f);
    //         }
    //     }
    // }
}