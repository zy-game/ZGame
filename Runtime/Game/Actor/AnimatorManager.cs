using System;
using UnityEngine;

namespace ZGame.Game
{
    public class AnimatorManager : IDisposable
    {
        private Animator _animator;
        public AnimatorManager(Animator animator)
        {
            _animator = animator;
        }

        public void OnUpdate()
        {
            
        }

        public void Dispose()
        {
        }
    }
}