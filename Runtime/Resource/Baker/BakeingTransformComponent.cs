using System;
using System.Linq;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace ZGame.Resource
{
    [BakeingReference(typeof(Transform))]
    public class BakeingTransformComponent : IBakeingHandle
    {
        private static ulong _id = 1;

        public void OnBakeing(Component component, ref Entity entity, ref EntityCommandBuffer ecb, World world)
        {
            ecb.AddComponent(entity, LocalTransform.FromPositionRotationScale(component.transform.position, component.transform.rotation, 1));
            ecb.AddComponent(entity, new LocalToWorld()
            {
                Value = component.transform.localToWorldMatrix
            });
            ecb.AddComponent(entity, new Prefab());
            Func<Entity, bool> func = x => world.EntityManager.HasComponent<SceneReference>(x);
            var sceneReference = world.EntityManager.GetAllEntities().First(func);
            SceneReference sr = world.EntityManager.GetComponentData<SceneReference>(sceneReference);
            ecb.AddSharedComponent(entity, new SceneTag()
            {
                SceneEntity = sceneReference
            });
            ecb.AddComponent(entity, new EntityGuid()
            {
                a = _id,
                b = ~_id
            });
            ecb.AddSharedComponent(entity, new SceneSection()
            {
                SceneGUID = sr.SceneGUID,
                Section = 0
            });
            _id++;

            if (component.transform.parent == null)
            {
                return;
            }

            ecb.AddComponent(entity, new CompanionLink()
            {
                Companion = component.transform.parent.gameObject
            });
        }
    }
}