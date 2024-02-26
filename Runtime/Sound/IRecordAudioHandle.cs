using System;

namespace ZGame.Sound
{
    public interface IRecordAudioHandle : IDisposable
    {
        void StartRecord();
        void StopRecord();
        void Recording(byte[] bytes);


        public static IRecordAudioHandle OnCreate(Action<byte[]> callback)
        {
            return new Default(callback);
        }

        class Default : IRecordAudioHandle
        {
            private Action<byte[]> callback;

            public Default(Action<byte[]> callback)
            {
                this.callback = callback;
            }

            public void Dispose()
            {
                callback = null;
            }

            public void StartRecord()
            {
            }

            public void StopRecord()
            {
            }

            public void Recording(byte[] bytes)
            {
                if (callback is null)
                {
                    return;
                }

                callback(bytes);
            }
        }
    }
}