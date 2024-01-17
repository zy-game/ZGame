using System;
using UnityEngine;
using ZGame.Resource;

namespace ZGame.Game
{
    public class ActorEntity : IDisposable
    {
        private string _id;
        private string _name;
        private GameObject _gameObject;
        private SkinManager _skinManager;
        private StateManager _stateManager;
        private AnimatorManager _animatorManager;
        public string id => _id;
        public string name => _name;
        public GameObject gameObject => _gameObject;
        public SkinManager skinManager => _skinManager;
        public StateManager stateManager => _stateManager;
        public AnimatorManager animatorManager => _animatorManager;


        public static T Create<T>(string name, string modelPath) where T : ActorEntity
        {
            Debug.Log(modelPath);
            ResObject resObject = ResourceManager.instance.LoadAsset(modelPath);
            if (resObject.IsSuccess() is false)
            {
                return default;
            }

            T actor = Activator.CreateInstance<T>();
            actor._id = ID.GetString();
            actor._name = name;
            actor._gameObject = resObject.Instantiate();
            Animator animator = actor._gameObject.GetComponentInChildren<Animator>();
            if (animator != null)
            {
                actor._animatorManager = new AnimatorManager(animator);
            }

            actor._skinManager = new SkinManager(actor._gameObject);
            actor._stateManager = new StateManager(actor);
            return actor;
        }

        public virtual void OnUpdate()
        {
            if (_animatorManager != null)
            {
                _animatorManager.OnUpdate();
            }

            if (_stateManager != null)
            {
                _stateManager.OnUpdate();
            }
        }

        public void Dispose()
        {
            if (_gameObject != null)
            {
                GameObject.DestroyImmediate(_gameObject);
                _gameObject = null;
            }

            if (_skinManager != null)
            {
                _skinManager.Dispose();
                _skinManager = null;
            }

            if (_stateManager != null)
            {
                _stateManager.Dispose();
                _stateManager = null;
            }

            if (_animatorManager != null)
            {
                _animatorManager.Dispose();
                _animatorManager = null;
            }
        }
    }
}