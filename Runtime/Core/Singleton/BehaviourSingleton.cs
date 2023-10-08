using UnityEngine;
using UnityEngine.Events;

namespace ZEngine
{
    public class BehaviourSingleton<T> : MonoBehaviour where T : BehaviourSingleton<T>, new()
    {
        public static T instance => SingletonHandle.GetInstance();

        internal class SingletonHandle
        {
            private static T _instance;
            private static GameObject gameObject;

            public static T GetInstance()
            {
                if (_instance is not null)
                {
                    return _instance;
                }

                gameObject = new GameObject(typeof(T).Name);
                GameObject.DontDestroyOnLoad(gameObject);
                _instance = gameObject.AddComponent<T>();
                return _instance;
            }
        }
    }
}