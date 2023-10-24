using System;
using System.Collections.Generic;
using UnityEngine;

namespace ZGame
{
    public interface ISystem : IEntity
    {
        void Startup();
        void Shutdown();

        private static Dictionary<Type, ISystem> _systems = new Dictionary<Type, ISystem>();

        public static T Require<T>() where T : ISystem
        {
            if (_systems.TryGetValue(typeof(T), out ISystem system) is false)
            {
                _systems.Add(typeof(T), system = Activator.CreateInstance<T>());
                system.Startup();
            }

            return (T)system;
        }

        public static void Release<T>() where T : ISystem
        {
            ISystem system = Require<T>();
            if (system is null)
            {
                return;
            }

            system.Shutdown();
            system.Dispose();
            _systems.Remove(typeof(T));
        }

        public static void RegisterSystem<T>() where T : ISystem
        {
            if (_systems.TryGetValue(typeof(T), out ISystem system) is false)
            {
                _systems.Add(typeof(T), system = Activator.CreateInstance<T>());
            }
        }

        public static void Quit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}