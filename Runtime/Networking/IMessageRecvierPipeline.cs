using System;

namespace ZGame.Networking
{
    public interface IMessageRecvierPipeline : IEntity
    {
        void Recvier(IChannel channel, IMessage message);

        public static IMessageRecvierPipeline Create<T>(Action<T> callback) where T : IMessage
        {
            return new ActionMessageRecvier<T>(callback);
        }

        class ActionMessageRecvier<T> : IMessageRecvierPipeline where T : IMessage
        {
            private Action<T> callback;
            public string guid { get; } = ID.New();

            public ActionMessageRecvier(Action<T> callback)
            {
                this.callback = callback;
            }

            public void Dispose()
            {
                this.callback = null;
                GC.SuppressFinalize(this);
            }

            public void Recvier(IChannel channel, IMessage message)
            {
                if (typeof(T).IsAssignableFrom(message.GetType()) is false)
                {
                    return;
                }

                callback?.Invoke((T)message);
            }
        }
    }
}