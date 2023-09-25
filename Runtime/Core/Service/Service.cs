using System;
using System.Collections.Generic;
using UnityEngine;

namespace ZEngine
{
    public interface IService : IDisposable
    {
        void Awake();
        void Update();
        void Execute(ICommand command);
    }

    public sealed class Service
    {
        private static Dictionary<Type, IService> services = new Dictionary<Type, IService>();

        public static void Initialize()
        {
            
        }

        public static void Schedule(ICommand command)
        {
        }

        public static T GetService<T>() where T : IService => (T)GetService(typeof(T));

        public static IService GetService(Type type)
        {
            if (typeof(IService).IsAssignableFrom(type) is false)
            {
                Engine.Console.Log(new NotImplementedException(nameof(IService)));
                return default;
            }

            if (services.TryGetValue(type, out IService service) is false)
            {
                service = (IService)Activator.CreateInstance(type);
                service.Awake();
                services.Add(type, service);
            }

            return service;
        }

        public static void UnloadService<T>() where T : IService => UnloadService(typeof(T));

        public static void UnloadService(Type type)
        {
            if (services.TryGetValue(type, out IService service) is false)
            {
                Debug.Log($"没有找到指定类型的服务{type}");
                return;
            }

            service.Dispose();
            services.Remove(type);
        }
    }
}