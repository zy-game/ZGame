using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using Newtonsoft.Json;

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

        public static void SerializeToFile(object data, string path)
        {
            string json = Serialize(data);
            if (json.IsNullOrEmpty())
            {
                return;
            }

            File.WriteAllText(path, json);
        }

        public static string Serialize(object options)
        {
            if (options is null)
            {
                return String.Empty;
            }

            return JsonConvert.SerializeObject(options, new JsonSerializerSettings()
            {
                Converters = new List<JsonConverter>()
                {
                    new OptionsConverter()
                }
            });
        }

        public static T Deserialize<T>(string body)
        {
            if (body.IsNullOrEmpty())
            {
                return default;
            }

            return JsonConvert.DeserializeObject<T>(body, new JsonSerializerSettings()
            {
                Converters = new List<JsonConverter>()
                {
                    new OptionsConverter()
                }
            });
        }

        public static T DeserializeFileData<T>(string path)
        {
            if (File.Exists(path) is false)
            {
                return default;
            }

            return Deserialize<T>(File.ReadAllText(path));
        }

        public class OptionsConverter : JsonConverter
        {
            /// <summary>
            ///     写入Json数据
            /// </summary>
            /// <param name="writer"></param>
            /// <param name="value"></param>
            /// <param name="serializer"></param>
            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                string json = string.Empty;
                Type type = value.GetType();
                if (type.IsArray is false && typeof(IList).IsAssignableFrom(type) is false)
                {
                    OpData opData = new OpData();
                    opData.type = value.GetType().FullName;
                    opData.value = JsonConvert.SerializeObject(value);
                    json = JsonConvert.SerializeObject(opData);
                    writer.WriteValue(json);
                    return;
                }

                IList list = type.IsArray ? ((Array)value) : (IList)value;
                List<OpData> opDatas = new List<OpData>();
                for (int i = 0; i < list.Count; i++)
                {
                    opDatas.Add(new OpData()
                    {
                        type = list[i].GetType().FullName,
                        value = JsonConvert.SerializeObject(list[i])
                    });
                }

                json = JsonConvert.SerializeObject(opDatas);
                writer.WriteValue(json);
            }

            /// <summary>
            ///     读Json数据
            /// </summary>
            /// <param name="reader"></param>
            /// <param name="objectType"></param>
            /// <param name="existingValue"></param>
            /// <param name="serializer"></param>
            /// <returns></returns>
            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                string strValue = reader.Value.ToString();
                IList list = default;
                if (objectType.IsArray is false && typeof(IList).IsAssignableFrom(objectType) is false)
                {
                    OpData opData = JsonConvert.DeserializeObject<OpData>(strValue);
                    Type type = AppDomain.CurrentDomain.FindType(opData.type);
                    if (type is null)
                    {
                        return default;
                    }

                    return JsonConvert.DeserializeObject(opData.value, type);
                }

                Type m = objectType.IsArray ? objectType.GetElementType() : objectType.GetGenericArguments()[0];
                list = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(m));
                List<OpData> listOpData = JsonConvert.DeserializeObject<List<OpData>>(strValue);
                for (int i = 0; i < listOpData.Count; i++)
                {
                    Type target = AppDomain.CurrentDomain.FindType(listOpData[i].type);
                    if (target is null)
                    {
                        ZGame.Console.Log("没找到类型：", listOpData[i].type);
                        continue;
                    }

                    object v = JsonConvert.DeserializeObject(listOpData[i].value, target);
                    list.Add(v);
                }

                if (objectType.IsArray is false)
                {
                    return list;
                }

                Array array = Array.CreateInstance(objectType.GetElementType(), list.Count);
                list.CopyTo(array, 0);
                return array;
            }

            /// <summary>
            ///     是否可以转换
            /// </summary>
            /// <param name="objectType"></param>
            /// <returns></returns>
            public override bool CanConvert(Type objectType)
            {
                if (objectType.IsArray && typeof(IOptions).IsAssignableFrom(objectType.GetElementType()))
                {
                    return true;
                }

                if (typeof(IList).IsAssignableFrom(objectType) && typeof(IOptions).IsAssignableFrom(objectType.GetGenericArguments()[0]))
                {
                    return true;
                }

                return objectType == typeof(IOptions);
            }

            class OpData
            {
                public string type;
                public string value;
            }
        }
    }
}