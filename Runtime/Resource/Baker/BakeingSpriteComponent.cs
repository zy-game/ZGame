using Unity.Collections;
using Unity.Entities;
using Unity.Rendering;
using UnityEngine;
using UnityEngine.U2D;

namespace ZGame.Resource
{
    [BakeingReference(typeof(SpriteRenderer))]
    public class BakeingSpriteComponent : IBakeingHandle
    {
        public void OnBakeing(Component component, ref Entity entity, ref EntityCommandBuffer ecb, World world)
        {
            SpriteRenderer sr = component as SpriteRenderer;
            if (sr.sprite == null)
            {
                return;
            }

            SpriteRenderer sp = Object.Instantiate(sr);
            CompanionLink companionLink = new CompanionLink()
            {
                Companion = sp.gameObject
            };


            ecb.AddComponent(entity, companionLink.Clone());
            ecb.AddComponent(entity, companionLink);
        }
    }
}