using UnityEngine;

namespace ZGame.Game
{
    public class EntityTransformComponent : IComponent
    {
        public GameObject gameObject { get; set; }
        public Transform transform => gameObject.transform;

        public void Release()
        {
            GameObject.DestroyImmediate(gameObject);
        }
    }
}