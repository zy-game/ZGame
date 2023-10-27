using System;
using System.Collections;
using UnityEngine;

namespace ZGame
{
    public sealed class Behaviour : MonoBehaviour
    {
        private static Behaviour _instance;

        public static Behaviour instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new GameObject("Behaviour").AddComponent<Behaviour>();
                }

                return _instance;
            }
        }
    }

    public static class Extension
    {
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
    }
}