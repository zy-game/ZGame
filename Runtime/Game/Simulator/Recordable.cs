using System.Collections.Generic;
using System.Linq;

namespace ZGame.Game
{
    /// <summary>
    /// 数据记录器
    /// </summary>
    public class Recordable : IReferenceObject
    {
        public List<FrameData> frameDataList = new();
        private int frame;


        public FrameData GetFrameData()
        {
            if (frame < frameDataList.Count)
            {
                return frameDataList[frame++];
            }

            if (frameDataList.Count == 0)
            {
                GameFrameworkEntry.Logger.Log("没有帧，预测");
                FrameData frameData = GameFrameworkFactory.Spawner<FrameData>();
                frameData.frame = frame++;
                // AddFrameData(frameData);
                return frameData;
            }

            return frameDataList.LastOrDefault();
        }

        public void Reset(int frame)
        {
            this.frame = frame;
        }

        public void AddFrameData(FrameData frameData)
        {
            this.frameDataList.Add(frameData);
        }

        public void Clear()
        {
            frameDataList.Clear();
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