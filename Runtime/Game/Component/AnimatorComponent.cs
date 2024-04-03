using System.Collections.Generic;
using UnityEngine;

namespace ZGame.Game
{
    public class AnimatorComponent : IComponent
    {
        public Animator animator;

        public void Release()
        {
            animator = null;
        }
    }
}