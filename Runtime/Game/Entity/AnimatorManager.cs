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

        public void Switch(string stateName)
        {
            _animator.Play(stateName);
        }

        public void Switch(string stateName, int layer)
        {
            _animator.Play(stateName, layer);
        }

        public void Switch(string stateName, int layer, float normalizedTime)
        {
            _animator.Play(stateName, layer, normalizedTime);
        }

        public void Dispose()
        {
        }
    }
}