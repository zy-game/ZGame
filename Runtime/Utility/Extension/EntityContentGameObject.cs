using UnityEngine;
using ZEngine.World;

namespace ZEngine
{
    sealed class EntityContentGameObject : MonoBehaviour
    {
        public TransformComponent transformComponent;

        public PhysicsComponent physicsComponent;

        private void FixedUpdate()
        {
            if (transformComponent is null)
            {
                return;
            }

            this.transform.position = transformComponent.position;
            this.transform.rotation = Quaternion.Euler(transformComponent.rotation);
            this.transform.localScale = transformComponent.scale;
        }

        private void OnCollisionEnter(Collision other)
        {
            EntityContentGameObject entityContentGameObject = other.gameObject.GetComponent<EntityContentGameObject>();
            if (entityContentGameObject == null || physicsComponent is null)
            {
                return;
            }

            physicsComponent?.OnEntry(entityContentGameObject.physicsComponent);
            entityContentGameObject.physicsComponent?.OnEntry(physicsComponent);
        }

        private void OnCollisionEnter2D(Collision2D other)
        {
            EntityContentGameObject entityContentGameObject = other.gameObject.GetComponent<EntityContentGameObject>();
            if (entityContentGameObject == null || physicsComponent is null)
            {
                return;
            }

            physicsComponent?.OnEntry(entityContentGameObject.physicsComponent);
            entityContentGameObject.physicsComponent?.OnEntry(physicsComponent);
        }

        private void OnCollisionExit(Collision other)
        {
            EntityContentGameObject entityContentGameObject = other.gameObject.GetComponent<EntityContentGameObject>();
            if (entityContentGameObject == null || physicsComponent is null)
            {
                return;
            }

            physicsComponent?.OnExit(entityContentGameObject.physicsComponent);
            entityContentGameObject.physicsComponent?.OnExit(physicsComponent);
        }

        private void OnCollisionExit2D(Collision2D other)
        {
            EntityContentGameObject entityContentGameObject = other.gameObject.GetComponent<EntityContentGameObject>();
            if (entityContentGameObject == null || physicsComponent is null)
            {
                return;
            }

            physicsComponent?.OnExit(entityContentGameObject.physicsComponent);
            entityContentGameObject.physicsComponent?.OnExit(physicsComponent);
        }

        private void OnCollisionStay(Collision other)
        {
            EntityContentGameObject entityContentGameObject = other.gameObject.GetComponent<EntityContentGameObject>();
            if (entityContentGameObject == null || physicsComponent is null)
            {
                return;
            }

            physicsComponent?.OnStay(entityContentGameObject.physicsComponent);
            entityContentGameObject.physicsComponent?.OnStay(physicsComponent);
        }

        private void OnCollisionStay2D(Collision2D other)
        {
            EntityContentGameObject entityContentGameObject = other.gameObject.GetComponent<EntityContentGameObject>();
            if (entityContentGameObject == null || physicsComponent is null)
            {
                return;
            }

            physicsComponent?.OnStay(entityContentGameObject.physicsComponent);
            entityContentGameObject.physicsComponent?.OnStay(physicsComponent);
        }

        private void OnTriggerEnter(Collider other)
        {
            EntityContentGameObject entityContentGameObject = other.gameObject.GetComponent<EntityContentGameObject>();
            if (entityContentGameObject == null || physicsComponent is null)
            {
                return;
            }

            physicsComponent?.OnEntry(entityContentGameObject.physicsComponent);
            entityContentGameObject.physicsComponent?.OnEntry(physicsComponent);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            EntityContentGameObject entityContentGameObject = other.gameObject.GetComponent<EntityContentGameObject>();
            if (entityContentGameObject == null || physicsComponent is null)
            {
                return;
            }

            physicsComponent?.OnEntry(entityContentGameObject.physicsComponent);
            entityContentGameObject.physicsComponent?.OnEntry(physicsComponent);
        }

        private void OnTriggerExit(Collider other)
        {
            EntityContentGameObject entityContentGameObject = other.gameObject.GetComponent<EntityContentGameObject>();
            if (entityContentGameObject == null || physicsComponent is null)
            {
                return;
            }

            physicsComponent?.OnExit(entityContentGameObject.physicsComponent);
            entityContentGameObject.physicsComponent?.OnExit(physicsComponent);
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            EntityContentGameObject entityContentGameObject = other.gameObject.GetComponent<EntityContentGameObject>();
            if (entityContentGameObject == null || physicsComponent is null)
            {
                return;
            }

            physicsComponent?.OnExit(entityContentGameObject.physicsComponent);
            entityContentGameObject.physicsComponent?.OnExit(physicsComponent);
        }

        private void OnTriggerStay(Collider other)
        {
            EntityContentGameObject entityContentGameObject = other.gameObject.GetComponent<EntityContentGameObject>();
            if (entityContentGameObject == null || physicsComponent is null)
            {
                return;
            }

            physicsComponent?.OnStay(entityContentGameObject.physicsComponent);
            entityContentGameObject.physicsComponent?.OnStay(physicsComponent);
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            EntityContentGameObject entityContentGameObject = other.gameObject.GetComponent<EntityContentGameObject>();
            if (entityContentGameObject == null || physicsComponent is null)
            {
                return;
            }

            physicsComponent?.OnStay(entityContentGameObject.physicsComponent);
            entityContentGameObject.physicsComponent?.OnStay(physicsComponent);
        }
    }
}