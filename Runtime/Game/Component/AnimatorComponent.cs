using System.Collections.Generic;
using UnityEngine;

namespace ZGame.Game
{
    public class AnimatorComponent : IComponent
    {
        public Animator animator;

        public override void OnAwake(params object[] args)
        {
            GameObjectComponent gameObjectComponent = this.entity.GetComponent<GameObjectComponent>();
            if (gameObjectComponent is null)
            {
                return;
            }

            animator = gameObjectComponent.gameObject.GetComponentInChildren<Animator>();
            if (animator != null)
            {
                return;
            }

            animator = gameObjectComponent.gameObject.AddComponent<Animator>();
        }
    }
}