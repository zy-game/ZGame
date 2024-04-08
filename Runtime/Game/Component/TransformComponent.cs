using UnityEngine;

namespace ZGame.Game
{
    public class TransformComponent : IComponent
    {
        public uint id { get; set; }
        public GameObject gameObject { get; set; }
        public Transform transform => gameObject.transform;

        public void Release()
        {
            GameObject.DestroyImmediate(gameObject);
        }
    }
}