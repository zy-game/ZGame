using UnityEngine;

namespace ZEngine.Game
{
    /// <summary>
    /// 物理组件
    /// </summary>
    public interface IEntityPhysicsComponent : IEntityComponent
    {
        void Entry(IEntity entity);

        void Exit(IEntity entity);

        void Stay(IEntity entity);

        public static T Create<T>(IEntity entity) where T : IEntityPhysicsComponent
        {
            TransformComponent transformComponent = entity.GetComponent<TransformComponent>();
            if (transformComponent is null)
            {
                return default;
            }

            T physicsComponent = entity.AddComponent<T>();
            Linker linker = transformComponent.gameObject.AddComponent<Linker>();
            linker.entity = entity;
            linker.entityPhysicsComponent = physicsComponent;
            return physicsComponent;
        }

        class Linker : MonoBehaviour
        {
            public IEntity entity;
            public IEntityPhysicsComponent entityPhysicsComponent;

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

            private void Entry(GameObject gameObject)
            {
                Linker linker = gameObject.GetComponent<Linker>();
                if (linker == null || linker.entity is null || linker.entityPhysicsComponent is null)
                {
                    return;
                }

                linker.entityPhysicsComponent.Entry(this.entity);
                entityPhysicsComponent.Entry(linker.entity);
            }

            private void Exit(GameObject gameObject)
            {
                Linker linker = gameObject.GetComponent<Linker>();
                if (linker == null || linker.entity is null || linker.entityPhysicsComponent is null)
                {
                    return;
                }

                linker.entityPhysicsComponent.Exit(this.entity);
                entityPhysicsComponent.Exit(linker.entity);
            }

            private void Stay(GameObject gameObject)
            {
                Linker linker = gameObject.GetComponent<Linker>();
                if (linker == null || linker.entity is null || linker.entityPhysicsComponent is null)
                {
                    return;
                }

                linker.entityPhysicsComponent.Stay(this.entity);
                entityPhysicsComponent.Stay(linker.entity);
            }
        }
    }
}