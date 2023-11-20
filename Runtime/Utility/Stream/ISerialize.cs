using System;

namespace Runtime.Core
{
    public interface ISerialize : IDisposable
    {
        void Serialize(IWriter writer);
        void Deserialize(IReader reader);
    }
}