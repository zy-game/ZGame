using System;
using Unity.Entities;
using Unity.Entities.Serialization;
using UnityEngine;

namespace ZGame.Game.Baker
{
    public class PrefabAutoBaker : AutoBaker
    {
        [SerializeField] public BakerData bakerData;

        protected override void OnBaker(IBaker baker)
        {
            var gameObject = bakerData.gameObject;
            var components = bakerData.components;
            var entity = baker.GetEntity(TransformUsageFlags.None);
            var reference = new EntityPrefabReference(gameObject);
            baker.AddComponent(entity, new PrefabReference()
            {
                reference = reference
            });
            foreach (var component in components)
            {
                baker.AddComponent(entity, AppDomain.CurrentDomain.GetType(component));
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