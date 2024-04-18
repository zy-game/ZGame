using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ZGame
{
    public partial class Extension
    {
        /// <summary>
        /// 在当前程序域中获取指定名称的类型
        /// </summary>
        /// <param name="domain"></param>
        /// <param name="name"></param>
        /// <returns></returns>
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

        /// <summary>
        /// 获取当前程序域中所有子类
        /// </summary>
        /// <param name="domain"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static List<Type> GetAllSubClasses<T>(this AppDomain domain)
        {
            return domain.GetAllSubClasses(typeof(T));
        }


        /// <summary>
        /// 获取当前程序域中所有子类
        /// </summary>
        /// <param name="domain"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
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

        /// <summary>
        /// 获取当前程序集中所有子类
        /// </summary>
        /// <param name="assembly"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
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

        /// <summary>
        /// 获取当前程序域中所有指定类型的特性
        /// </summary>
        /// <param name="domain"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
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

        /// <summary>
        /// 获取当前程序域中所有指定类型的特性
        /// </summary>
        /// <param name="domain"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
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

        /// <summary>
        /// 获取所有指定了特定特性的类型
        /// </summary>
        /// <param name="domain"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
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

        /// <summary>
        /// 获取指定类型的静态字段
        /// </summary>
        /// <param name="domain"></param>
        /// <param name="typeName"></param>
        /// <param name="fieldName"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
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

        /// <summary>
        /// 在程序集中创建自定类型
        /// </summary>
        /// <param name="assembly"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <exception cref="EntryPointNotFoundException"></exception>
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