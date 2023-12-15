using System;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Entities.Serialization;
using Unity.Scenes;
using UnityEditorInternal;
using UnityEngine;

namespace ZGame.Game.Baker
{
    public class MultiAutoBaker : AutoBaker
    {
#if UNITY_EDITOR
        public AssemblyDefinitionAsset asset;
#endif
        [SerializeField] public List<BakerData> bakerDataList;

        protected override void OnBaker(IBaker baker)
        {
            foreach (var bakerData in bakerDataList)
            {
                var gameObject = bakerData.gameObject;
                var components = bakerData.components;
                var entity = baker.GetEntity(TransformUsageFlags.Dynamic);
                var reference = new EntityPrefabReference(gameObject);

                baker.AddComponent(entity, new RequestEntityPrefabLoaded()
                {
                    Prefab = reference
                });
                baker.AddComponent(entity, new PrefabReference()
                {
                    reference = reference
                });
                HashSet<string> componentSet = new HashSet<string>();
                foreach (var component in components)
                {
                    if (componentSet.Contains(component))
                    {
                        continue;
                    }

                    baker.AddComponent(entity, AppDomain.CurrentDomain.GetType(component));
                    componentSet.Add(component);
                }
            }
        }

        public override Type GetComponentType()
        {
            return typeof(IComponentData);
        }

        private void OnEnable()
        {
        }
    }
}