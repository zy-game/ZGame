using Unity.Entities;
using Unity.Entities.Serialization;
using Unity.Scenes;
using UnityEngine;
using ZGame.Game.Baker;

namespace OdinGame.Scripts
{
    public class EntitySpawner : MonoBehaviour
    {
        public GameObject target;

        class EntitySpawnerBaker : Baker<EntitySpawner>
        {
            public override void Bake(EntitySpawner authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.None);
                var prefabEntity = new EntityPrefabReference(authoring.target);

                AddComponent(entity, new RequestEntityPrefabLoaded()
                {
                    Prefab = prefabEntity
                });
                AddComponent(entity, new PrefabReference()
                {
                    reference = prefabEntity
                });
            }
        }
    }
}