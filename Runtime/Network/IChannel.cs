namespace ZEngine.Network
{
    public interface IChannel : IReference
    {
        string address { get; }

        bool connected { get; }
        void Close();
        void Connect(string address);
        IWriteResult WriteAndFlush(IMessagePacket messagePackage);
    }

    public interface IWriteResult : IReference
    {
        IChannel channel { get; }
        IMessagePacket message { get; }
        Status status { get; }

        public static IWriteResult Create(IChannel channel, IMessagePacket package, Status status)
        {
            ChannelWriteBufferResult channelWriteBufferResult = Engine.Class.Loader<ChannelWriteBufferResult>();
            channelWriteBufferResult.channel = channel;
            channelWriteBufferResult.message = package;
            channelWriteBufferResult.status = status;
            return channelWriteBufferResult;
        }

        class ChannelWriteBufferResult : IWriteResult
        {
            public void Release()
            {
                channel = null;
                message = null;
                status = Status.None;
            }

            public IChannel channel { get; set; }
            public IMessagePacket message { get; set; }
            public Status status { get; set; }
        }
    }
}