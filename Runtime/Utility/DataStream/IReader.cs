using System;
using System.IO;

namespace ZGame.DataStream
{
    public interface IReader : IDisposable
    {
        long Length { get; }
        long Position { get; }
        int Read(byte[] buffer, int offset, int count);
        bool ReadBoolean();
        byte ReadByte();
        char ReadChar();
        decimal ReadDecimal();
        double ReadDouble();
        short ReadInt16();
        int ReadInt32();
        long ReadInt64();
        string ReadString();
        ushort ReadUInt16();
        uint ReadUInt32();
        ulong ReadUInt64();
        float ReadSingle();
        void Reset();
        void Seek(long offset, SeekOrigin origin);
    }
}