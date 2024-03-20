using UnityEngine;

namespace ZGame.Game.Common
{
    public class GameObjectComponent : EntityComponent
    {
        private GameObject _gameObject;

        public GameObject gameObject => _gameObject;

        public Transform transform => _gameObject.transform;


        public override async void OnAwake(params object[] args)
        {
            if (args is null || args.Length == 0)
            {
                return;
            }

            string prefabName = args[0] as string;
            _gameObject = await GameFrameworkEntry.Resource.LoadGameObjectAsync(prefabName);
        }

        public override void OnEnable()
        {
            this.gameObject.SetActive(true);
        }

        public override void OnDisable()
        {
            this.gameObject.SetActive(false);
        }

        public override void Dispose()
        {
            if (_gameObject != null)
            {
                GameObject.DestroyImmediate(_gameObject);
                _gameObject = null;
            }
        }

        public void SetParent(Transform parent, Vector3 localPosition, Vector3 localEulerAngles, Vector3 localScale)
        {
            _gameObject.transform.SetParent(parent, false);
            _gameObject.transform.localPosition = localPosition;
            _gameObject.transform.localEulerAngles = localEulerAngles;
            _gameObject.transform.localScale = localScale;
        }
    }
}