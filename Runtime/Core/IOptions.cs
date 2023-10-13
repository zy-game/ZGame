using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace ZEngine
{
    /// <summary>
    /// 配置项
    /// </summary>
    public interface IOptions : IDisposable
    {
        int id { get; }
        string name { get; }
        string describe { get; }

        public static T Deserialize<T>(string body) where T : IOptions
        {
            if (body.IsNullOrEmpty())
            {
                return default;
            }

            body = body.Replace("\n\r\\", "");
            int index = 0;
            Type type = typeof(T);
            if (body.StartsWith("{") && body.EndsWith("}"))
            {
                return (T)ReadObject(type, body, ref index);
            }

            object result = default;
            if (body.StartsWith("[") && body.EndsWith("]"))
            {
                if (type.IsArray || typeof(IList).IsAssignableFrom(type))
                {
                    List<T> map = ReadArray<T>(body);
                    result = type.IsArray ? map.ToArray() : map;
                }
            }

            return (T)result;
        }

        private static List<T> ReadArray<T>(string body)
        {
            List<T> list = new List<T>();
            int index = 1;
            while (index < body.Length)
            {
                list.Add((T)ReadObject(typeof(T), body, ref index));
            }

            return list;
        }

        private static object ReadObject(Type objType, string body, ref int index)
        {
            int count = 0;
            if (body[index].Equals('{') is false)
            {
                return default;
            }

            object result = Activator.CreateInstance(objType);
            List<FieldData> fieldDatas = new List<FieldData>();
            index++;
            bool isStart = false;
            while (true)
            {
                if (body[index].Equals('{'))
                {
                    index++;
                    isStart = true;
                    continue;
                }

                if (body[index].Equals('}'))
                {
                    index++;
                    if (isStart is false)
                    {
                        break;
                    }

                    isStart = false;
                    continue;
                }

                if (body[index].Equals(','))
                {
                    index++;
                    continue;
                }

                string name = ReadString(body, ref index);
                index++;
                string type = ReadString(body, ref index);
                index++;
                string value = ReadString(body, ref index);
                FieldInfo fieldInfo = objType.GetField(name);
                if (fieldInfo is not null)
                {
                    fieldInfo.SetValue(result, Convert.ChangeType(value, fieldInfo.FieldType));
                    continue;
                }

                PropertyInfo propertyInfo = objType.GetProperty(name);
                if (propertyInfo is not null)
                {
                    propertyInfo.SetValue(result, Convert.ChangeType(value, propertyInfo.PropertyType));
                }
            }

            return result;
        }

        private static string ReadString(string body, ref int index)
        {
            if (body[index].Equals('"') is false)
            {
                return String.Empty;
            }

            body = body.Replace("\n\r\\", "");
            index++;
            int nextSplit = body.IndexOf('"', index);
            int count = nextSplit - index;
            string result = body.Substring(index, count);
            if (result.IsNullOrEmpty())
            {
                index--;
                return String.Empty;
            }

            index = index + count + 1;
            return result;
        }

        public static string Serialize(string path, IOptions options)
        {
            return String.Empty;
        }

        class FieldData
        {
            public string name;
            public string type;
            public string value;
        }
    }

    public enum Localtion
    {
        /// <summary>
        /// 内部配置选项，在打包时会将不在Resources目录下的配置拷贝至Resources中
        /// </summary>
        Internal,

        /// <summary>
        /// 项目级配置选项,在打包时这个配置不会被打进包内
        /// </summary>
        Project,

        /// <summary>
        /// 热更配置项，这个配置只存在包内，在加载时只会从Bundle包中加载
        /// </summary>
        Packaged,
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class ConfigAttribute : Attribute
    {
        internal string path;
        internal Localtion localtion;

        public ConfigAttribute(Localtion localtion, string path = "")
        {
            this.path = path;
            this.localtion = localtion;
        }
    }

    public class InternalConfigAttribute : ConfigAttribute
    {
        public InternalConfigAttribute() : base(Localtion.Internal)
        {
        }
    }

    public class ProjectConfigAttribute : ConfigAttribute
    {
        public ProjectConfigAttribute() : base(Localtion.Project)
        {
        }
    }

    public class PackageConfigAttribute : ConfigAttribute
    {
        public PackageConfigAttribute(string path) : base(Localtion.Packaged, path)
        {
        }
    }

    public class OptionsName : Attribute
    {
        public string name;

        public OptionsName(string name)
        {
            this.name = name;
        }
    }
}