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
            if (frame >= frameDataList.Count)
            {
                if (frameDataList.Count == 0)
                {
                    FrameData frameData = GameFrameworkFactory.Spawner<FrameData>();
                    frameData.frame = frame;
                    AddFrameData(frameData);
                }

                return frameDataList.LastOrDefault();
            }

            return frameDataList[frame++];
        }

        public void Reset(int frame)
        {
            this.frame = frame;
        }

        public void AddFrameData(FrameData frameData)
        {
            int index = frameDataList.FindIndex(x => x.frame == frameData.frame);
            if (index >= 0)
            {
                frameDataList[index] = frameData;
                frame = frameData.frame;
                GameFrameworkEntry.Logger.Log($"回滚帧到：{frame}");
            }
            else
            {
                this.frameDataList.Add(frameData);
            }
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