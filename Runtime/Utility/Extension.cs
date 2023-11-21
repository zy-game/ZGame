using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;

namespace ZGame
{
    public static partial class Extension
    {
        public static void OnDestroyEventCallback(this GameObject gameObject, UnityAction callback)
        {
            EventListener listener = gameObject.GetComponent<EventListener>();
            if (listener == null)
            {
                listener = gameObject.AddComponent<EventListener>();
            }

            listener.onDestroy.AddListener(callback);
        }

        public static string GetPlatformName()
        {
#if UNITY_ANDROID
            return "android";
#elif UNITY_IPHONE
            return "ios";
#endif
            return "windows";
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
            => string.IsNullOrEmpty(str);

        public static Type GetTypeForThat(this AppDomain domain, string name)
        {
            foreach (var VARIABLE in domain.GetAssemblies())
            {
                Type type = VARIABLE.GetType(name);
                if (type is null)
                {
                    continue;
                }

                return type;
            }

            return default;
        }

        public static T CreateInstance<T>(this AppDomain appDomain, string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return default;
            }

            Type type = appDomain.GetTypeForThat(name);
            if (type is null)
            {
                return default;
            }

            return (T)Activator.CreateInstance(type);
        }

        public static List<Type> GetAllTypes<T>(this AppDomain domain)
        {
            List<Type> result = new List<Type>();
            Type baseType = typeof(T);
            foreach (var VARIABLE in domain.GetAssemblies())
            {
                foreach (var VARIABLE2 in VARIABLE.GetTypes())
                {
                    if (baseType.IsAssignableFrom(VARIABLE2) && VARIABLE2.IsInterface is false && VARIABLE2.IsAbstract is false)
                    {
                        result.Add(VARIABLE2);
                    }
                }
            }

            return result;
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


        public static Type FindType(this Assembly assembly, string name)
        {
            if (assembly is not null)
            {
                foreach (var VARIABLE in assembly.GetTypes())
                {
                    if (VARIABLE.Name == name)
                    {
                        return VARIABLE;
                    }
                }
            }

            return default;
        }
    }
}