using System;
using UnityEngine;
using ZEngine.Resource;

namespace ZEngine.Game
{
    /// <summary>
    /// 实体组件
    /// </summary>
    public interface IEntityComponent : IReference
    {
    }

    /// <summary>
    /// 物理组件
    /// </summary>
    public class PhysicsComponent : IEntityComponent
    {
        public void Release()
        {
        }

        public virtual void Entry(IEntity entity)
        {
        }

        public virtual void Exit(IEntity entity)
        {
        }

        public virtual void Stay(IEntity entity)
        {
        }

        class PhysicsLinker : MonoBehaviour
        {
            private IEntity entity;
            private PhysicsComponent physicsComponent;

            public void Content(IEntity entity)
            {
                this.entity = entity;
                this.physicsComponent = entity.GetComponent<PhysicsComponent>();
            }

            private void Entry(GameObject gameObject)
            {
                PhysicsLinker linker = gameObject.GetComponent<PhysicsLinker>();
                if (linker is null || linker.entity is null || linker.physicsComponent is null || entity is null || physicsComponent is null)
                {
                    return;
                }

                physicsComponent.Entry(linker.entity);
                linker.physicsComponent.Entry(entity);
            }

            private void Exit(GameObject gameObject)
            {
                PhysicsLinker linker = gameObject.GetComponent<PhysicsLinker>();
                if (linker is null || linker.entity is null || linker.physicsComponent is null || entity is null || physicsComponent is null)
                {
                    return;
                }

                physicsComponent.Exit(linker.entity);
                linker.physicsComponent.Exit(entity);
            }

            private void Stay(GameObject gameObject)
            {
                PhysicsLinker linker = gameObject.GetComponent<PhysicsLinker>();
                if (linker is null || linker.entity is null || linker.physicsComponent is null || entity is null || physicsComponent is null)
                {
                    return;
                }

                physicsComponent.Stay(linker.entity);
                linker.physicsComponent.Stay(entity);
            }

            private void OnCollisionEnter(Collision other)
            {
                Entry(other.gameObject);
            }

            private void OnCollisionEnter2D(Collision2D other)
            {
                Entry(other.gameObject);
            }

            private void OnCollisionExit(Collision other)
            {
                Exit(other.gameObject);
            }

            private void OnCollisionExit2D(Collision2D other)
            {
                Exit(other.gameObject);
            }

            private void OnCollisionStay(Collision other)
            {
                Stay(other.gameObject);
            }

            private void OnCollisionStay2D(Collision2D other)
            {
                Stay(other.gameObject);
            }

            private void OnTriggerEnter(Collider other)
            {
                Entry(other.gameObject);
            }

            private void OnTriggerEnter2D(Collider2D other)
            {
                Entry(other.gameObject);
            }

            private void OnTriggerExit(Collider other)
            {
                Exit(other.gameObject);
            }

            private void OnTriggerExit2D(Collider2D other)
            {
                Exit(other.gameObject);
            }

            private void OnTriggerStay(Collider other)
            {
                Stay(other.gameObject);
            }

            private void OnTriggerStay2D(Collider2D other)
            {
                Stay(other.gameObject);
            }
        }
    }

    /// <summary>
    /// 变换组件
    /// </summary>
    public class TransformComponent : IEntityComponent
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
            IRequestAssetExecute<GameObject> requestAssetExecuteHandle = Engine.Resource.LoadAsset<GameObject>(assetPath);
            return default;
        }

        public static TransformComponent Create(IEntity entity, GameObject gameObject)
        {
            TransformComponent transformComponent = entity.AddComponent<TransformComponent>();
            transformComponent._gameObject = gameObject;
            return transformComponent;
        }

        public void Release()
        {
            GameObject.DestroyImmediate(gameObject);
            position = Vector3.zero;
            rotation = Vector3.zero;
            scale = Vector3.zero;
        }
    }
}