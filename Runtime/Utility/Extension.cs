using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;

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

    public sealed class GameObjectDestoryCallback
    {
        class Listener : MonoBehaviour
        {
            public UnityEvent callback = new UnityEvent();

            private void OnDestroy()
            {
                callback.Invoke();
            }
        }

        public static void Create(GameObject gameObject, UnityAction callback)
        {
            Listener listener = gameObject.GetComponent<Listener>();
            if (listener == null)
            {
                listener = gameObject.AddComponent<Listener>();
            }

            listener.callback.AddListener(callback);
        }
    }

    public static class Extension
    {
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