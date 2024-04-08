using TrueSync;
using UnityEngine;

namespace ZGame.Game
{
    public class FrameSyncComponent : IComponent
    {
        public uint id;
        public TSVector origin;
        public TSTransform transform;
        public TSRigidBody rigidBody;

        public void Release()
        {
        }
    }
}