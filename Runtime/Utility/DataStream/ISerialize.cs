using System;

namespace ZGame.DataStream
{
    public interface ISerialize : IDisposable
    {
        void Serialize(IWriter writer);
        void Deserialize(IReader reader);
    }
}