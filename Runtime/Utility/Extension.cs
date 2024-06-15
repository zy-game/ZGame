using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;


namespace ZGame
{
    public static partial class Extension
    {
        public static TimeSpan ToTimeSpan(this DateTime dateTime)
        {
            return new TimeSpan(dateTime.Day, dateTime.Hour, dateTime.Minute, dateTime.Second, dateTime.Millisecond);
        }

        /// <summary>
        /// 对lenght进行最大均分
        /// </summary>
        /// <param name="lenght"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static int MaxSharinCount(int lenght, int count)
        {
            int max = lenght / count;
            if (max * count < lenght)
            {
                max++;
            }

            return max;
        }

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

        public static int GetLoopCount(int value, int min, int max)
        {
            if (value > max)
            {
                return min;
            }

            if (value < min)
            {
                return max;
            }

            return value;
        }

        public static void CopyTextToClipboard(this string textToCopy)
        {
            TextEditor editor = new TextEditor();
            editor.text = textToCopy;
            editor.Copy();
        }

        public static long GetHash(this byte[] data, int index, int length)
        {
            const int p = 16777619;
            long hash = 2166136261L;

            for (int i = index; i < index + length; i++)
            {
                hash = (hash ^ data[i]) * p;
            }

            hash += hash << 13;
            hash ^= hash >> 7;
            hash += hash << 3;
            hash ^= hash >> 17;
            hash += hash << 5;
            return hash;
        }
    }
}