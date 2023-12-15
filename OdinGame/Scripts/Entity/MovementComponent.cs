using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Scenes;
using Unity.Transforms;
using UnityEngine;
using ZGame.Game.Baker;
using Random = Unity.Mathematics.Random;

namespace OdinGame.Scripts
{
    public struct MovementComponent : IComponentData
    {
    }
    //实现多个prefab baker
    

    [BurstCompile]
    public partial struct MoveSystem : ISystem
    {
        private float timer;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<PrefabReference>();
            state.RequireForUpdate<PrefabLoadResult>();
        }

        public void OnUpdate(ref SystemState state)
        {
            timer -= SystemAPI.Time.DeltaTime;
            if (timer > 0)
            {
                Debug.Log("create");
                return;
            }

            var config = SystemAPI.GetSingleton<PrefabReference>();
            timer = 1;

            var configEntity = SystemAPI.GetSingletonEntity<PrefabReference>();
            var prefabLoadResult = SystemAPI.GetComponent<PrefabLoadResult>(configEntity);
            var entity = state.EntityManager.Instantiate(prefabLoadResult.PrefabRoot);
            var random = Random.CreateFromIndex((uint)state.GlobalSystemVersion);
            state.EntityManager.SetComponentData(entity, LocalTransform.FromPosition(random.NextFloat(-5, 5), random.NextFloat(-5, 5), 0));
        }
    }
}