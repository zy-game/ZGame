using System.Text;

namespace ZGame
{
    public class URL
    {
        public static string UrlEncode(string str) => str == null ? (string)null : UrlEncode(str, Encoding.UTF8);
        public static string UrlEncode(string str, Encoding e) => str == null ? (string)null : Encoding.ASCII.GetString(UrlEncodeToBytes(str, e));

        public static byte[] UrlEncodeToBytes(string str, Encoding e)
        {
            if (str == null)
                return (byte[])null;
            byte[] bytes = e.GetBytes(str);
            return UrlEncodeBytesToBytesInternal(bytes, 0, bytes.Length, false);
        }

        private static byte[] UrlEncodeBytesToBytesInternal(
            byte[] bytes,
            int offset,
            int count,
            bool alwaysCreateReturnValue)
        {
            int num1 = 0;
            int num2 = 0;
            for (int index = 0; index < count; ++index)
            {
                char ch = (char)bytes[offset + index];
                if (ch == ' ')
                    ++num1;
                else if (!IsSafe(ch))
                    ++num2;
            }

            if (!alwaysCreateReturnValue && num1 == 0 && num2 == 0)
                return bytes;
            byte[] bytesInternal = new byte[count + num2 * 2];
            int num3 = 0;
            for (int index1 = 0; index1 < count; ++index1)
            {
                byte num4 = bytes[offset + index1];
                char ch = (char)num4;
                if (IsSafe(ch))
                    bytesInternal[num3++] = num4;
                else if (ch == ' ')
                {
                    bytesInternal[num3++] = (byte)43;
                }
                else
                {
                    byte[] numArray1 = bytesInternal;
                    int index2 = num3;
                    int num5 = index2 + 1;
                    numArray1[index2] = (byte)37;
                    byte[] numArray2 = bytesInternal;
                    int index3 = num5;
                    int num6 = index3 + 1;
                    int hex1 = (int)(byte)IntToHex((int)num4 >> 4 & 15);
                    numArray2[index3] = (byte)hex1;
                    byte[] numArray3 = bytesInternal;
                    int index4 = num6;
                    num3 = index4 + 1;
                    int hex2 = (int)(byte)IntToHex((int)num4 & 15);
                    numArray3[index4] = (byte)hex2;
                }
            }

            return bytesInternal;
        }

        internal static bool IsSafe(char ch)
        {
            if (ch >= 'a' && ch <= 'z' || ch >= 'A' && ch <= 'Z' || ch >= '0' && ch <= '9')
                return true;
            switch (ch)
            {
                case '!':
                case '\'':
                case '(':
                case ')':
                case '*':
                case '-':
                case '.':
                case '_':
                    return true;
                default:
                    return false;
            }
        }

        private static int HexToInt(char h)
        {
            if (h >= '0' && h <= '9')
                return (int)h - 48;
            if (h >= 'a' && h <= 'f')
                return (int)h - 97 + 10;
            return h < 'A' || h > 'F' ? -1 : (int)h - 65 + 10;
        }

        internal static char IntToHex(int n) => n <= 9 ? (char)(n + 48) : (char)(n - 10 + 97);
    }
}