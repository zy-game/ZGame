using System;
using UnityEngine;

namespace ZGame.Game
{
    public class BoxColliderComponent : IComponent
    {
        public class BoxColliderHandler : MonoBehaviour
        {
            public BoxColliderComponent component;

            public void OnTriggerEnter(Collider other)
            {
                OnEntry(other.gameObject);
            }

            private void OnTriggerStay(Collider other)
            {
                OnStay(other.gameObject);
            }

            private void OnTriggerExit(Collider other)
            {
                OnExit(other.gameObject);
            }

            private void OnTriggerEnter2D(Collider2D other)
            {
                OnEntry(other.gameObject);
            }

            private void OnTriggerExit2D(Collider2D other)
            {
                OnExit(other.gameObject);
            }

            private void OnTriggerStay2D(Collider2D other)
            {
                OnStay(other.gameObject);
            }

            private void OnCollisionEnter(Collision other)
            {
                OnEntry(other.gameObject);
            }

            private void OnCollisionExit(Collision other)
            {
                OnExit(other.gameObject);
            }

            private void OnCollisionStay(Collision other)
            {
                OnStay(other.gameObject);
            }

            private void OnCollisionEnter2D(Collision2D other)
            {
                OnEntry(other.gameObject);
            }

            private void OnCollisionExit2D(Collision2D other)
            {
                OnExit(other.gameObject);
            }

            private void OnCollisionStay2D(Collision2D other)
            {
                OnStay(other.gameObject);
            }

            private void OnEntry(GameObject target)
            {
                if (target == null)
                {
                    return;
                }

                if (target.TryGetComponent<BoxColliderHandler>(out BoxColliderHandler handler) is false)
                {
                    return;
                }

                component.OnEnter(handler.component.entity);
            }

            private void OnStay(GameObject target)
            {
                if (target == null)
                {
                    return;
                }

                if (target.TryGetComponent<BoxColliderHandler>(out BoxColliderHandler handler) is false)
                {
                    return;
                }

                component.OnStay(handler.component.entity);
            }

            private void OnExit(GameObject target)
            {
                if (target == null)
                {
                    return;
                }

                if (target.TryGetComponent<BoxColliderHandler>(out BoxColliderHandler handler) is false)
                {
                    return;
                }

                component.OnExit(handler.component.entity);
            }
        }

        public override void OnAwake(params object[] args)
        {
            GameObjectComponent gameObjectComponent = this.entity.GetComponent<GameObjectComponent>();
            if (gameObjectComponent is null)
            {
                return;
            }

            if (gameObjectComponent.gameObject.TryGetComponent<BoxColliderHandler>(out BoxColliderHandler handler) is false)
            {
                handler = gameObjectComponent.gameObject.AddComponent<BoxColliderHandler>();
            }

            handler.component = this;
        }

        /// <summary>
        /// 进入碰撞区
        /// </summary>
        /// <param name="entity"></param>
        public virtual void OnEnter(GameEntity entity)
        {
        }

        /// <summary>
        /// 停留在碰撞区
        /// </summary>
        /// <param name="entity"></param>
        public virtual void OnStay(GameEntity entity)
        {
        }

        /// <summary>
        /// 退出碰撞区
        /// </summary>
        /// <param name="entity"></param>
        public virtual void OnExit(GameEntity entity)
        {
        }
    }
}