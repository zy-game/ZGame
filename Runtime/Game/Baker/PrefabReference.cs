using Unity.Entities;
using Unity.Entities.Serialization;

namespace ZGame.Game.Baker
{
    public struct PrefabReference : IComponentData
    {
        public EntityPrefabReference reference;
    }
}