using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

public class SelectorAttribute : Attribute
{
    public string ValuesGetter;

    public SelectorAttribute(string getter)
    {
        ValuesGetter = getter;
    }
}

namespace ZGame
{
    /// <summary>
    /// 资源模式
    /// </summary>
    public enum ResourceMode : byte
    {
        Editor,
        Simulator,
        Internal
    }

    public enum CodeMode
    {
        Native,
        Hotfix,
    }

    public enum Status : byte
    {
        None,
        Success,
        Fail,
        Runing,
    }

    public static partial class Extension
    {
        private static Stopwatch sw = new Stopwatch();

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

        public static void StartSample()
        {
            sw.Restart();
        }

        public static long GetSampleTime()
        {
            sw.Stop();
            return sw.ElapsedMilliseconds;
        }

        public static void StopSample(string format)
        {
            sw.Stop();
            ZG.Logger.Log(string.Format(format, sw.ElapsedMilliseconds));
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


        public static Type GetType(this AppDomain domain, string name)
        {
            foreach (var VARIABLE in domain.GetAssemblies())
            {
                foreach (var VARIABLE2 in VARIABLE.GetTypes())
                {
                    if (VARIABLE2.Name == name || VARIABLE2.FullName == name)
                    {
                        return VARIABLE2;
                    }
                }
            }

            return default;
        }

        public static List<Type> GetAllSubClasses<T>(this AppDomain domain)
        {
            return domain.GetAllSubClasses(typeof(T));
        }


        public static List<Type> GetAllSubClasses(this AppDomain domain, Type parent)
        {
            List<Type> result = new List<Type>();
            foreach (var VARIABLE in domain.GetAssemblies())
            {
                foreach (var VARIABLE2 in VARIABLE.GetTypes())
                {
                    if (parent.IsAssignableFrom(VARIABLE2) && VARIABLE2.IsInterface is false && VARIABLE2.IsAbstract is false)
                    {
                        result.Add(VARIABLE2);
                    }
                }
            }

            return result;
        }

        public static List<Type> GetAllSubClasses<T>(this Assembly assembly)
        {
            List<Type> result = new List<Type>();
            Type baseType = typeof(T);
            foreach (var VARIABLE in assembly.GetTypes())
            {
                if (baseType.IsAssignableFrom(VARIABLE) && VARIABLE.IsInterface is false && VARIABLE.IsAbstract is false)
                {
                    result.Add(VARIABLE);
                }
            }

            return result;
        }

        public static Dictionary<Type, T> GetCustomAttributeMap<T>(this AppDomain domain) where T : Attribute
        {
            Dictionary<Type, T> map = new Dictionary<Type, T>();
            foreach (var assembly in domain.GetAssemblies())
            {
                foreach (var type in assembly.GetTypes())
                {
                    T attribute = type.GetCustomAttribute<T>();
                    if (attribute is null)
                    {
                        continue;
                    }

                    map.Add(type, attribute);
                }
            }

            return map;
        }

        public static List<T> GetCustomAttributes<T>(this AppDomain domain) where T : Attribute
        {
            List<T> result = new List<T>();
            foreach (var VARIABLE in domain.GetAssemblies())
            {
                foreach (var VARIABLE2 in VARIABLE.GetTypes())
                {
                    T[] attribute = VARIABLE2.GetCustomAttributes<T>().ToArray();

                    if (attribute != null && attribute.Length > 0)
                    {
                        result.AddRange(attribute);
                    }
                }
            }

            return result;
        }

        public static List<Type> GetCustomAttributesWithoutType<T>(this AppDomain domain) where T : Attribute
        {
            List<Type> result = new List<Type>();
            foreach (var VARIABLE in domain.GetAssemblies())
            {
                foreach (var VARIABLE2 in VARIABLE.GetTypes())
                {
                    T[] attribute = VARIABLE2.GetCustomAttributes<T>().ToArray();

                    if (attribute != null && attribute.Length > 0)
                    {
                        result.Add(VARIABLE2);
                    }
                }
            }

            return result;
        }

        public static T GetStaticFieldValue<T>(this AppDomain domain, string typeName, string fieldName)
        {
            Type type = AppDomain.CurrentDomain.GetType(typeName);
            if (type is null)
            {
                return default;
            }

            object target = Activator.CreateInstance(type);
            FieldInfo fieldInfo = type.GetField(fieldName);
            if (fieldInfo is null)
            {
                return default;
            }

            return (T)fieldInfo.GetValue(target);
        }

        public static T CreateInstance<T>(this Assembly assembly)
        {
            Type entryType = assembly.GetAllSubClasses<T>().FirstOrDefault();
            if (entryType is null)
            {
                throw new EntryPointNotFoundException();
            }

            T startup = (T)RefPooled.Spawner(entryType);
            if (startup is null)
            {
                throw new EntryPointNotFoundException();
            }

            return startup;
        }
    }
}