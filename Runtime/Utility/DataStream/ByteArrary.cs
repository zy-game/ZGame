using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ZGame.DataStream
{
    public static class ByteArray
    {
        private const int CopyThreshold = 12;

        public static void Copy(byte[] src, int srcOffset, byte[] dst, int dstOffset, int count)
        {
            if (count > 12)
            {
                Buffer.BlockCopy((Array)src, srcOffset, (Array)dst, dstOffset, count);
            }
            else
            {
                int num = srcOffset + count;
                for (int index = srcOffset; index < num; ++index)
                    dst[dstOffset++] = src[index];
            }
        }

        public static void Reverse(byte[] bytes)
        {
            int index1 = 0;
            for (int index2 = bytes.Length - 1; index1 < index2; --index2)
            {
                byte num = bytes[index1];
                bytes[index1] = bytes[index2];
                bytes[index2] = num;
                ++index1;
            }
        }
    }
}