using UnityEngine;
using ZEngine.Resource;

namespace ZEngine.Game
{
    /// <summary>
    /// 变换组件
    /// </summary>
    public class TransformComponent : IComponent
    {
        public Vector3 position
        {
            get
            {
                if (gameObject == null)
                {
                    return Vector3.zero;
                }

                return gameObject.transform.position;
            }
            set
            {
                if (gameObject == null)
                {
                    return;
                }

                gameObject.transform.position = value;
            }
        }

        public Vector3 rotation
        {
            get
            {
                if (gameObject == null)
                {
                    return Vector3.zero;
                }

                return gameObject.transform.rotation.eulerAngles;
            }
            set
            {
                if (gameObject == null)
                {
                    return;
                }

                gameObject.transform.rotation = Quaternion.Euler(value);
            }
        }

        public Vector3 scale
        {
            get
            {
                if (gameObject == null)
                {
                    return Vector3.zero;
                }

                return gameObject.transform.localScale;
            }
            set
            {
                if (gameObject == null)
                {
                    return;
                }

                gameObject.transform.localScale = value;
            }
        }

        private GameObject _gameObject;
        public GameObject gameObject => _gameObject;


        public static TransformComponent Create(IEntity entity, string assetPath)
        {
            IRequestAssetObjectSchedule<GameObject> requestAssetObjectSchedule = Engine.Resource.LoadAsset<GameObject>(assetPath);
            if (requestAssetObjectSchedule.result == null)
            {
                return default;
            }

            return Create(entity, requestAssetObjectSchedule.Instantiate());
        }

        public static TransformComponent Create(IEntity entity, GameObject gameObject)
        {
            TransformComponent transformComponent = entity.AddComponent<TransformComponent>();
            transformComponent._gameObject = gameObject;
            return transformComponent;
        }

        public void Dispose()
        {
            GameObject.DestroyImmediate(gameObject);
            position = Vector3.zero;
            rotation = Vector3.zero;
            scale = Vector3.zero;
        }
    }
}