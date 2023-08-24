using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Random = Unity.Mathematics.Random;

namespace ZEngine.Game
{
    public struct PrefabSpawner : IComponentData
    {
        public Entity entity;
    }

    public class BakerPrefabToEntity : MonoBehaviour
    {
        public GameObject prefab;

        class BakerToEntity : Baker<BakerPrefabToEntity>
        {
            public override void Bake(BakerPrefabToEntity authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);
                AddComponent(entity, new PrefabSpawner
                {
                    entity = GetEntity(authoring.prefab, TransformUsageFlags.Dynamic),
                });
            }
        }
    }

    [BurstCompile]
    public partial struct SpawnerSystem : ISystem
    {
        uint updateCounter;

        public void OnCreate(ref SystemState state)
        {
        }

        public void OnDestroy(ref SystemState state)
        {
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            foreach (RefRW<PrefabSpawner> spawner in SystemAPI.Query<RefRW<PrefabSpawner>>())
            {
                ProcessSpawner(ref state, spawner);
            }
        }

        private void ProcessSpawner(ref SystemState state, RefRW<PrefabSpawner> spawner)
        {
            Entity newEntity = state.EntityManager.Instantiate(spawner.ValueRO.entity);
            var random = Random.CreateFromIndex(updateCounter++);
            state.EntityManager.SetComponentData(newEntity, LocalTransform.FromPosition((random.NextFloat3() - new float3(0.5f, 0, 0.5f)) * 20));
        }
    }
}