using System;
using System.Threading.Tasks;

namespace ZEngine.Network
{
    public interface IChannel : IDisposable
    {
        string address { get; }

        bool connected { get; }
        IScheduleHandle<IChannel> Close();
        IScheduleHandle<IChannel> Connect(string address, int id = 0);
        IScheduleHandle<int> WriteAndFlush(byte[] bytes);
    }
}