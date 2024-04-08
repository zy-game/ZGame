using System.Collections.Generic;

namespace ZGame.Game
{
    /// <summary>
    /// 数据记录器
    /// </summary>
    public class Recordable : IReferenceObject
    {
        public List<SyncData> data = new();

        /// <summary>
        /// 从指定帧删除
        /// </summary>
        /// <param name="frame"></param>
        public void RemoveRange(long frame)
        {
            data.RemoveAll(x => x.frame >= frame);
        }

        public SyncData GetFrameData(long frame)
        {
            return data.Find(x => x.frame == frame);
        }

        public void AddFrameData(SyncData data)
        {
            this.data.Add(data);
        }

        public void Clear()
        {
            data.Clear();
        }

        public byte[] Encode()
        {
            return null;
        }

        public void Release()
        {
        }
    }
}