using System;

namespace ZEngine.Network
{
    public interface IMessageHandle : IDisposable
    {
        void Handle(IMessaged messaged);
    }
}