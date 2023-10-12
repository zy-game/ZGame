using System;
using System.Collections.Generic;
using System.Reflection;

namespace ZEngine
{
    public static class AssemblyExtension
    {
        public static Type FindType(this AppDomain domain, string name)
        {
            foreach (var VARIABLE in domain.GetAssemblies())
            {
                Type t = VARIABLE.GetType(name);
                if (t is null)
                {
                    continue;
                }

                return t;
            }

            return default;
        }

        public static Type[] FindTypes<T>(this AppDomain domain)
        {
            Type source = typeof(T);
            List<Type> types = new List<Type>();
            foreach (var VARIABLE in domain.GetAssemblies())
            {
                foreach (var t in VARIABLE.GetTypes())
                {
                    if (source.IsAssignableFrom(t) is false)
                    {
                        continue;
                    }

                    types.Add(t);
                }
            }

            return types.ToArray();
        }

        public static Type[] FindTypes<T>(this Assembly assembly)
        {
            Type source = typeof(T);
            List<Type> types = new List<Type>();
            foreach (var t in assembly.GetTypes())
            {
                if (source.IsAssignableFrom(t) is false)
                {
                    continue;
                }

                types.Add(t);
            }

            return types.ToArray();
        }
    }
}