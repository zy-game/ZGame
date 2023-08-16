using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ZEngine
{
    class UnityEventHandle : MonoBehaviour
    {
        private Dictionary<GameEventType, GameEventSubscrbe<UnityEventArgs>> map = new Dictionary<GameEventType, GameEventSubscrbe<UnityEventArgs>>();

        public void Subscribe(GameEventType type, GameEventSubscrbe<UnityEventArgs> subscribe)
        {
            if (!map.TryGetValue(type, out GameEventSubscrbe<UnityEventArgs> handle))
            {
                map.Add(type, subscribe);
                return;
            }

            handle += subscribe;
        }

        public void Unsubscribe(GameEventType type, GameEventSubscrbe<UnityEventArgs> subscribe)
        {
            if (!map.TryGetValue(type, out GameEventSubscrbe<UnityEventArgs> handle))
            {
                return;
            }

            handle -= subscribe;
        }

        private void Execute(GameEventType eventType, object data = null)
        {
            if (!map.TryGetValue(eventType, out GameEventSubscrbe<UnityEventArgs> subscrbe))
            {
                return;
            }

            Engine.Console.Log("Mono Event", eventType);
            UnityEventArgs args = Engine.Class.Loader<UnityEventArgs>();
            args.gameObject = this.gameObject;
            args.eventType = eventType;
            args.data = data;
            subscrbe.Execute(args);
        }

        private void Update()
        {
            Execute(GameEventType.Update);
        }

        private void FixedUpdate()
        {
            Execute(GameEventType.FixedUpdate);
        }

        private void LateUpdate()
        {
            Execute(GameEventType.LateUpdate);
        }

        private void OnEnable()
        {
            Execute(GameEventType.OnEnable);
        }

        private void OnDisable()
        {
            Execute(GameEventType.OnDisable);
        }

        private void OnDestroy()
        {
            Execute(GameEventType.OnDestroy);
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            Execute(GameEventType.OnApplicationFocus, hasFocus);
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            Execute(GameEventType.OnApplicationPause, pauseStatus);
        }

        private void OnBecameInvisible()
        {
            Execute(GameEventType.OnBecameInvisible);
        }

        private void OnBecameVisible()
        {
            Execute(GameEventType.OnBecameVisible);
        }

        private void OnCollisionEnter(Collision other)
        {
            Execute(GameEventType.OnCollisionEnter, other);
        }

        private void OnCollisionEnter2D(Collision2D other)
        {
            Execute(GameEventType.OnCollisionEnter2D, other);
        }

        private void OnCollisionExit(Collision other)
        {
            Execute(GameEventType.OnCollisionExit, other);
        }

        private void OnCollisionExit2D(Collision2D other)
        {
            Execute(GameEventType.OnCollisionExit2D, other);
        }

        private void OnCollisionStay(Collision other)
        {
            Execute(GameEventType.OnCollisionStay, other);
        }

        private void OnCollisionStay2D(Collision2D other)
        {
            Execute(GameEventType.OnCollisionStay2D, other);
        }

        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            Execute(GameEventType.OnControllerColliderHit, hit);
        }

        private void OnDrawGizmos()
        {
            Execute(GameEventType.OnDrawGizmos);
        }

        private void OnDrawGizmosSelected()
        {
            Execute(GameEventType.OnDrawGizmosSelected);
        }

        private void OnTriggerEnter(Collider other)
        {
            Execute(GameEventType.OnTriggerEnter, other);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            Execute(GameEventType.OnTriggerEnter2D, other);
        }

        private void OnTriggerExit(Collider other)
        {
            Execute(GameEventType.OnTriggerExit, other);
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            Execute(GameEventType.OnTriggerExit2D, other);
        }

        private void OnTriggerStay(Collider other)
        {
            Execute(GameEventType.OnTriggerStay, other);
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            Execute(GameEventType.OnTriggerStay2D, other);
        }

        public void OnApplicationQuit()
        {
            Execute(GameEventType.OnApplicationQuit);
        }
    }
}