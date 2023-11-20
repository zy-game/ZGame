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
}