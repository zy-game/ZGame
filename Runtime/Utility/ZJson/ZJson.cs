using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Newtonsoft.Json;

namespace ZEngine.ZJson
{
    public sealed class JsonOptions
    {
        internal List<IZJsonConverter> converters = new List<IZJsonConverter>();

        public bool isZip { get; set; }
        public bool isEncrypt { get; set; }

        public void AddConverter(IZJsonConverter converter)
        {
        }

        public void RemoveConverter(IZJsonConverter converter)
        {
        }
    }

    public sealed class JsonWrite : IDisposable
    {
        public void Dispose()
        {
        }

        public override string ToString()
        {
            return base.ToString();
        }
    }

    public sealed class JsonRead : IDisposable
    {
        public void Initialize(string json)
        {
        }

        public void Dispose()
        {
        }

        public T Read<T>()
        {
            return default;
        }
    }

    public class ZJson
    {
        public static JsonOptions options { get; } = new JsonOptions()
        {
            converters = new List<IZJsonConverter>()
            {
                new InternalNumberSeriazedConverter(),
                new InternalJsonObjectSerializeConverter(),
                new InternalArraryObjectSerizlieConverter(),
            }
        };

        public static string ToJson(object data)
        {
            Analysis serialize = Activator.CreateInstance<Analysis>();
            string result = serialize.ToJson(data);
            serialize.Dispose();
            return result;
        }

        public static T Parse<T>(string data)
        {
            return (T)Parse(data, typeof(T));
        }

        public static object Parse(string data, Type type)
        {
            Analysis serialize = Activator.CreateInstance<Analysis>();
            object result = serialize.Parse(data, type);
            serialize.Dispose();
            return result;
        }


        class Analysis : IDisposable
        {
            public void Dispose()
            {
            }

            public object Parse(string data, Type type)
            {
                JsonRead read = Activator.CreateInstance<JsonRead>();
                read.Initialize(data);
                for (int i = 0; i < options.converters.Count; i++)
                {
                    object temp = options.converters[i].ReadJson(read, type);
                    if (temp == null)
                    {
                        continue;
                    }

                    return temp;
                }

                return default;
            }

            public string ToJson(object data)
            {
                JsonWrite write = Activator.CreateInstance<JsonWrite>();
                Type type = data.GetType();
                for (int i = 0; i < options.converters.Count; i++)
                {
                    options.converters[i].WriteJson(write, data);
                }

                return write.ToString();
            }
        }

        class InternalNumberSeriazedConverter : IZJsonConverter
        {
            public void Dispose()
            {
            }

            public void WriteJson(JsonWrite builder, object value)
            {
                Launche.Console.Log(value.GetType());
            }

            public object ReadJson(JsonRead read, Type objectType)
            {
                switch (Type.GetTypeCode(objectType))
                {
                    case TypeCode.Byte:
                        return read.Read<byte>();
                    case TypeCode.Double:
                        return read.Read<double>();
                    case TypeCode.Int16:
                        return read.Read<short>();
                    case TypeCode.Int32:
                        return read.Read<int>();
                    case TypeCode.Int64:
                        return read.Read<long>();
                    case TypeCode.Single:
                        return read.Read<float>();
                    case TypeCode.SByte:
                        return read.Read<sbyte>();
                    case TypeCode.UInt16:
                        return read.Read<ushort>();
                    case TypeCode.UInt32:
                        return read.Read<uint>();
                    case TypeCode.UInt64:
                        return read.Read<ulong>();
                }

                return default;
            }
        }

        class InternalJsonObjectSerializeConverter : IZJsonConverter
        {
            public void Dispose()
            {
            }

            public void WriteJson(JsonWrite builder, object value)
            {
                if (value.GetType().IsArray is true)
                {
                    return;
                }

                Launche.Console.Log(value.GetType());
            }

            public object ReadJson(JsonRead read, Type objectType)
            {
                if (objectType.IsArray is true || typeof(IList).IsAssignableFrom(objectType))
                {
                    return default;
                }

                object result = Activator.CreateInstance(objectType);
                foreach (var VARIABLE in objectType.GetFields())
                {
                    foreach (var converter in ZJson.options.converters)
                    {
                        object data = converter.ReadJson(read, VARIABLE.FieldType);
                        if (data is null)
                        {
                            continue;
                        }

                        VARIABLE.SetValue(result, data);
                    }
                }

                Launche.Console.Log(objectType, objectType.IsArray, typeof(IList).IsAssignableFrom(objectType));
                return default;
            }
        }

        class InternalArraryObjectSerizlieConverter : IZJsonConverter
        {
            public void Dispose()
            {
            }

            public void WriteJson(JsonWrite builder, object value)
            {
                if (value.GetType().IsArray is false)
                {
                    return;
                }

                Launche.Console.Log(value.GetType());
            }

            public object ReadJson(JsonRead read, Type objectType)
            {
                if (objectType.IsArray is false && typeof(IList).IsAssignableFrom(objectType) is false)
                {
                    
                    return default;
                }

                Launche.Console.Log(objectType);
                return default;
            }
        }
    }

    public interface IZJsonConverter : IDisposable
    {
        void WriteJson(JsonWrite builder, object value);
        object ReadJson(JsonRead read, Type objectType);
    }
}