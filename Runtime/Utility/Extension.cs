using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

namespace ZGame
{
    public static partial class Extension
    {
        public static T CheckNotNull<T>(T value, string name) where T : class => (object)value != null ? value : throw new ArgumentNullException(name);

        internal static T CheckNotNullUnconstrained<T>(T value, string name) => (object)value != null ? value : throw new ArgumentNullException(name);

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

        public static T GetData<T>(this UnityWebRequest request)
        {
            if (request.result is not UnityWebRequest.Result.Success)
            {
                return default;
            }

            object _data = default;
            if (typeof(T) == typeof(string))
            {
                _data = request.downloadHandler.text;
            }
            else if (typeof(T) == typeof(byte[]))
            {
                _data = request.downloadHandler.data;
            }
            else if (typeof(T) is JObject)
            {
                _data = JObject.Parse(request.downloadHandler.text);
            }
            else
            {
                _data = JsonConvert.DeserializeObject<T>(request.downloadHandler.text);
            }

            return (T)_data;
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
    }
}