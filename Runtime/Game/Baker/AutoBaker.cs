using System;
using Unity.Entities;
using UnityEngine;

namespace ZGame.Game.Baker
{
    [DisallowMultipleComponent]
    public abstract class AutoBaker : MonoBehaviour
    {
        protected virtual void OnBaker(IBaker baker)
        {
            Debug.Log("=============");
        }
        
        /// <summary>
        /// The type of the ECS component to be authored.
        /// </summary>
        /// <returns>Returns the type of the ECS component.</returns>
        public abstract Type GetComponentType();

        [BakeDerivedTypes]
        class Baker : Baker<AutoBaker>
        {
            public override void Bake(AutoBaker authoring)
            {
                authoring.OnBaker(this);
            }
        }
    }
}