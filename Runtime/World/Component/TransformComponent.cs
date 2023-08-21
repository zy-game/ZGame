using UnityEngine;

namespace ZEngine.World
{
    public sealed class TransformComponent : EntityComponent
    {
        public Vector3 position;
        public Vector3 rotation;
        public Vector3 scale;
    }
}