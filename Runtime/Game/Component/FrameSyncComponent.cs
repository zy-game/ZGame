using TrueSync;
using UnityEngine;
using Vector2 = BEPUutilities.Vector2;

namespace ZGame.Game
{
    public class BoxColliderComponent : IComponent
    {
        /// <summary>
        /// 碰撞盒大小
        /// </summary>
        public Vector2 size;

        /// <summary>
        /// 碰撞盒中心点
        /// </summary>
        public Vector2 center;

        public void Release()
        {
        }
    }

    public class RigidbodyComponent : IComponent
    {
        
        public void Release()
        {
            
        }
    }

    public class FrameSyncComponent : IComponent
    {
        public uint id;
        public BEPUutilities.Vector3 origin;
        public Transform transform;
        public Rigidbody rigidBody;

        public void Release()
        {
        }
    }
}