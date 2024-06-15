using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace ZGame
{
    public static partial class Extension
    {
        public const string SPLIT = "%s";

        public static string FirstUpper(this string str)
        {
            //将str首字母转为大写
            return str.Substring(0, 1).ToUpper() + str.Substring(1);
        }

        public static bool IsEnglish(string input)
        {
            // 英文字符的Unicode范围是 U+0041 到 U+005A（大写）和 U+0061 到 U+007A（小写）  
            return input.All(ch => (ch >= 'A' && ch <= 'Z') || (ch >= 'a' && ch <= 'z'));
        }

        public static string RemoveNonEnglishCharacters(this string input)
        {
            // 正则表达式，匹配任何非英文字符并将其替换为空字符串  
            string pattern = @"[^a-zA-Z0-9\s]";
            return Regex.Replace(input, pattern, " ");
        }

        public static byte[] ToBytes(this string str)
        {
            return Encoding.UTF8.GetBytes(str);
        }

        public static string[] SplitToArrary(this string str, string split)
        {
            return str.Split(split).Where(x => x.IsNullOrEmpty() is false).ToArray();
        }

        public static bool EndsWith(this string str, params string[] suffix)
        {
            foreach (var VARIABLE in suffix)
            {
                if (str.EndsWith(VARIABLE))
                {
                    return true;
                }
            }

            return false;
        }

        public static bool IsValidEmail(this string email)
        {
            string pattern = @"^([a-zA-Z0-9_\-\.]+)@([a-zA-Z0-9_\-\.]+)\.([a-zA-Z]{2,5})$";
            return Regex.IsMatch(email, pattern);
        }

        public static bool IsValidCode(this string input, int lenght)
        {
            string pattern = $@"^\d{{{lenght}}}$"; // 匹配纯数字的的正则表达式  
            return Regex.IsMatch(input, pattern);
        }

        public static Color ToColor(this string hex)
        {
            return ColorUtility.TryParseHtmlString(hex, out Color color) ? color : Color.white;
        }

        public static string ToHex(this Color color)
        {
            return ColorUtility.ToHtmlStringRGB(color);
        }

        public static string Replace(this string value, params string[] t)
        {
            foreach (var VARIABLE in t)
            {
                value = value.Replace(VARIABLE, String.Empty);
            }

            return value;
        }

        public static bool IsNullOrEmpty(this string str)
        {
            return string.IsNullOrEmpty(str);
        }
    }
}