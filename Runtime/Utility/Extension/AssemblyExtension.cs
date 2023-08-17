using System.Collections.Generic;
using System;
using System.Reflection;

namespace ZEngine
{
    public static class AssemblyExtension
    {
        public static List<T> GetAttributes<T>(this AppDomain appdomain) where T : Attribute
        {
            List<T> result = new List<T>();
            foreach (var assembly in appdomain.GetAssemblies())
            {
                foreach (var type in assembly.GetTypes())
                {
                    T build = type.GetCustomAttribute<T>(true);
                    if (build is not null)
                    {
                        result.Add(build);
                    }
                }
            }

            return result;
        }

        public static Type GetTypeWithFullName(this AppDomain appDomain, string name)
        {
            foreach (var assembly in appDomain.GetAssemblies())
            {
                foreach (var type in assembly.GetTypes())
                {
                    if (type.FullName == name)
                    {
                        return type;
                    }
                }
            }

            return default;
        }

        public static List<Type> GetAttributesWithType<T>(this AppDomain appdomain) where T : Attribute
        {
            List<Type> result = new List<Type>();
            foreach (var assembly in appdomain.GetAssemblies())
            {
                foreach (var type in assembly.GetTypes())
                {
                    T build = type.GetCustomAttribute<T>(true);
                    if (build is not null)
                    {
                        result.Add(type);
                    }
                }
            }

            return result;
        }

        public static MethodInfo GetMethod(this Assembly assembly, string typeName, string name)
        {
            if (assembly is null || typeName.IsNullOrEmpty() || name.IsNullOrEmpty())
            {
                throw new ArgumentNullException();
            }

            Type type = assembly.GetType(typeName);
            if (type is null)
            {
                return default;
            }

            return type.GetMethod(name, BindingFlags.Public | BindingFlags.Static | BindingFlags.NonPublic);
        }

        public static object InvokeMethod(this Assembly assembly, string name, params object[] args)
        {
            if (assembly is null || name.IsNullOrEmpty())
            {
                throw new ArgumentNullException();
            }

            object target = default;
            MethodInfo method = default;
            BindingFlags flags = BindingFlags.Public | BindingFlags.Static | BindingFlags.NonPublic;
            foreach (var type in assembly.GetTypes())
            {
                if ((method = type.GetMethod(name, flags)) is not null)
                {
                    target = Activator.CreateInstance(type);
                    break;
                }
            }

            if (method is null)
            {
                return default;
            }

            return method.Invoke(target, new object[] { args });
        }
    }
}