using System.IO;

namespace ZGame
{
    public class Crc32
    {
        private static ulong[] Crc32Table;
        private static object _lock = new object();

        private static void GetCRC32Table()
        {
            ulong Crc;
            Crc32Table = new ulong[256];
            int i, j;
            for (i = 0; i < 256; i++)
            {
                Crc = (ulong)i;
                for (j = 8; j > 0; j--)
                {
                    if ((Crc & 1) == 1)
                        Crc = (Crc >> 1) ^ 0xEDB88320;
                    else
                        Crc >>= 1;
                }

                Crc32Table[i] = Crc;
            }
        }

        public static uint GetCRC32File(string filePath)
        {
            if (File.Exists(filePath) is false)
            {
                return 0;
            }

            return GetCRC32Str(File.ReadAllText(filePath));
        }

        public static uint GetCRC32Str(string sInputString, uint crc = 0)
        {
            if (Crc32Table is null)
            {
                GetCRC32Table();
            }

            uint crc32;
            byte[] buffer = System.Text.ASCIIEncoding.ASCII.GetBytes(sInputString);
            crc32 = crc ^ 0xffffffff;
            int len = buffer.Length;
            for (int i = 0; i < len; i++)
            {
                crc32 = (uint)((crc32 >> 8) ^ Crc32Table[(crc32 ^ buffer[i]) & 0xff]);
            }

            return crc32 ^ 0xffffffff;
        }

        public static uint GetCRC32Byte(byte[] buffer, uint crc = 0)
        {
            if (Crc32Table is null)
            {
                GetCRC32Table();
            }

            uint crc32;
            crc32 = crc ^ 0xffffffff;
            int len = buffer.Length;
            for (int i = 0; i < len; i++)
            {
                crc32 = (uint)((crc32 >> 8) ^ Crc32Table[(crc32 ^ buffer[i]) & 0xff]);
            }

            return crc32 ^ 0xffffffff;
        }

        public static uint GetCRC32Byte(byte[] buffer, int len, uint crc)
        {
            if (Crc32Table is null)
            {
                GetCRC32Table();
            }

            uint crc32;
            crc32 = crc ^ 0xffffffff;
            for (int i = 0; i < len; i++)
            {
                crc32 = (uint)((crc32 >> 8) ^ Crc32Table[(crc32 ^ buffer[i]) & 0xff]);
            }

            return crc32 ^ 0xffffffff;
        }

        public static uint GetCRC32Byte(byte[] buffer, int start, int len, uint crc)
        {
            if (Crc32Table is null)
            {
                GetCRC32Table();
            }

            uint crc32;
            crc32 = crc ^ 0xffffffff;
            for (int i = start; i < start + len; i++)
            {
                crc32 = (uint)((crc32 >> 8) ^ Crc32Table[(crc32 ^ buffer[i]) & 0xff]);
            }

            return crc32 ^ 0xffffffff;
        }


        public static uint Compute(byte[] bytes, int start, int size, uint crc = 0)
        {
            if (Crc32Table is null)
            {
                GetCRC32Table();
            }

            uint crc32;
            crc32 = crc ^ 0xffffffff;

            for (var i = start; i < start + size; i++)
            {
                crc32 = (uint)((crc32 >> 8) ^ Crc32Table[(crc32 ^ bytes[i]) & 0xff]);
            }

            return crc32 ^ 0xffffffff;
        }
    }
}