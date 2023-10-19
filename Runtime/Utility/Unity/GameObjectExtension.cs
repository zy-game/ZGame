using UnityEngine;
using UnityEngine.Events;

namespace ZEngine
{
    public static class GameObjectExtension
    {
        public static T TryGetComponent<T>(this GameObject gameObject) where T : Component
        {
            T linker = gameObject.GetComponent<T>();
            if (linker == null)
            {
                linker = gameObject.AddComponent<T>();
            }

            return linker;
        }

        public static void SetParent(this GameObject gameObject, GameObject parent, Vector3 position, Vector3 rotation, Vector3 scale)
        {
            if (gameObject == null || parent == null)
            {
                return;
            }

            gameObject.transform.SetParent(parent.transform);
            gameObject.transform.position = position;
            gameObject.transform.rotation = Quaternion.Euler(rotation);
            gameObject.transform.localScale = scale;
        }

        public static void OnDestroyEvent(this GameObject gameObject, UnityAction action)
        {
            Behaviour.OnDestroyGameObject(gameObject, action);
        }
    }
}