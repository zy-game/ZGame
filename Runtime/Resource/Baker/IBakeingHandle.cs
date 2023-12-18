using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace ZGame.Resource
{
    public interface IBakeingHandle
    {
        void OnBakeing(Component component, ref Entity entity, ref EntityCommandBuffer ecb, World world);

        private static Dictionary<Type, BakeingReference> bakeingReferences;

        public static Entity Baker(GameObject gameObject, World world)
        {
            EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.TempJob);
            if (bakeingReferences is null)
            {
                bakeingReferences = AppDomain.CurrentDomain.GetCustomAttributeMap<BakeingReference>();
            }

            Entity root = ecb.CreateEntity();
            BakeingComponent(gameObject, world, ref ecb, Entity.Null);
            ecb.Playback(world.EntityManager);
            ecb.Dispose();

            return root;
        }


        protected static void BakeingComponent(GameObject gameObject, World world, ref EntityCommandBuffer ecb, Entity root)
        {
            Component[] components = gameObject.GetComponents(typeof(Component));
            if (components == null || components.Length == 0)
            {
                return;
            }

            Entity entity = ecb.CreateEntity();
            foreach (var VARIABLE in bakeingReferences)
            {
                Component component = components.FirstOrDefault(x => x.GetType() == VARIABLE.Value.referenceType);
                if (component == null)
                {
                    continue;
                }

                IBakeingHandle bakeingHandle = Activator.CreateInstance(VARIABLE.Key) as IBakeingHandle;
                if (bakeingHandle is null)
                {
                    continue;
                }

                bakeingHandle.OnBakeing(component, ref entity, ref ecb, world);
            }

            if (gameObject.transform.parent != null)
            {
                ecb.AddComponent(entity, new Parent()
                {
                    Value = root
                });
                ecb.AddComponent(entity, new PreviousParent()
                {
                    Value = root
                });
            }

            if (gameObject.transform.childCount == 0)
            {
                return;
            }

            foreach (Transform child in gameObject.transform)
            {
                BakeingComponent(child.gameObject, world, ref ecb, entity);
            }
        }
    }
}