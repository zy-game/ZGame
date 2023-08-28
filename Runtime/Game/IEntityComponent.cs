using System;
using UnityEngine;

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
        public Vector3 position { get; set; }
        public Vector3 rotation { get; set; }
        public Vector3 scale { get; set; }

        private Action onRelease;

        public void Release()
        {
            onRelease?.Invoke();
            position = Vector3.zero;
            rotation = Vector3.zero;
            scale = Vector3.zero;
            onRelease = null;
        }


        class LinkerTransform : MonoBehaviour
        {
            public IEntity entity;
            private TransformComponent transformComponent;

            public void Content(IEntity entity)
            {
                this.entity = entity;
                this.transformComponent = entity.GetComponent<TransformComponent>();
                this.transformComponent.onRelease = () => { GameObject.DestroyImmediate(this.gameObject); };
            }

            private void FixedUpdate()
            {
                if (entity is null || transformComponent is null)
                {
                    return;
                }

                this.transform.position = transformComponent.position;
                this.transform.rotation = Quaternion.Euler(transformComponent.rotation);
                this.transform.localScale = transformComponent.scale;
            }
        }
    }
}