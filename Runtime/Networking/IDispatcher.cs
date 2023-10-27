using System;
using System.IO;
using Cysharp.Threading.Tasks;
using ProtoBuf;

namespace ZGame.Networking
{
    public interface IDispatcher : IEntity
    {
        void RecvieHandle(IChannel channel, uint opcode, MemoryStream memoryStream);

        public static IDispatcher Create<T>(Action<T> callback) where T : IMessage
        {
            return new ActionMessageRecvier<T>(callback);
        }

        public static IDispatcher Create<T>(UniTaskCompletionSource<T> taskCompletionSource) where T : IMessage
        {
            return Create<T>(args => taskCompletionSource.TrySetResult(args));
        }

        class ActionMessageRecvier<T> : IDispatcher where T : IMessage
        {
            private Action<T> callback;
            private uint opcode;
            public string guid { get; } = ID.New();

            public ActionMessageRecvier(Action<T> callback)
            {
                this.callback = callback;
                this.opcode = Crc32.GetCRC32Str(typeof(T).FullName);
            }

            public void Dispose()
            {
                this.callback = null;
                GC.SuppressFinalize(this);
            }

            public void RecvieHandle(IChannel channel, uint opcode, MemoryStream memoryStream)
            {
                if (this.opcode != opcode)
                {
                    return;
                }

                callback?.Invoke(Serializer.Deserialize<T>(memoryStream));
                CoreApi.networkSystem.UnsubscribeMessageRecvier<ActionMessageRecvier<T>>();
            }
        }
    }
}