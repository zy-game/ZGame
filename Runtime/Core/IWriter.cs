using System;
using System.IO;

namespace Runtime.Core
{
    public interface IWriter : IDisposable
    {
        void Write(byte[] buffer, int offset, int count);
        void Write(bool value);
        void Write(byte value);
        void Write(char value);
        void Write(decimal value);
        void Write(double value);
        void Write(short value);
        void Write(int value);
        void Write(long value);
        void Write(string value);
        void Write(ushort value);
        void Write(uint value);
        void Write(ulong value);
        void Write(float value);
        void Flush();
        void Reset();
        void Seek(long offset, SeekOrigin origin);
        long Length { get; }
        long Position { get; }
    }

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

    public interface ISerialize : IDisposable
    {
        void Serialize(IWriter writer);
        void Deserialize(IReader reader);
    }
}