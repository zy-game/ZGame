using System.Collections.Generic;
using UnityEngine;

namespace ZGame.Game.Common
{
    public class AnimatorComponent : EntityComponent
    {
        private Animator _animator;
        private Dictionary<string, object> _parameters;

        public override void OnAwake(params object[] args)
        {
            GameObjectComponent gameObjectComponent = this.entity.GetComponent<GameObjectComponent>();
            if (gameObjectComponent is null)
            {
                return;
            }

            _parameters = new Dictionary<string, object>();
            _animator = gameObjectComponent.gameObject.GetComponentInChildren<Animator>();
        }

        public void SetState(string stateName)
        {
            _animator.SetTrigger(stateName);
        }

        public void SetState(string parameterName, float value)
        {
            _animator.SetFloat(parameterName, value);
        }

        public void SetState(string parameterName, bool value)
        {
            _animator.SetBool(parameterName, value);
        }

        public void SetState(string parameterName, int value)
        {
            _animator.SetInteger(parameterName, value);
        }
    }
}