using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ZEngine
{
    class UnityEventHandle : MonoBehaviour
    {
        private ISubscribeHandle<UnityEventArgs> update;
        private ISubscribeHandle<UnityEventHandle> fixedupdate;
        private ISubscribeHandle<UnityEventArgs> lateupdate;
        private Dictionary<GameEventType, ISubscribeHandle<UnityEventArgs>> map = new Dictionary<GameEventType, ISubscribeHandle<UnityEventArgs>>();

        private static UnityEventHandle _instance;

        public static UnityEventHandle instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new GameObject("UnityEventHandle").AddComponent<UnityEventHandle>();
                    GameObject.DontDestroyOnLoad(_instance.gameObject);
                }

                return _instance;
            }
        }

        public void Subscribe(GameEventType type, ISubscribeHandle<UnityEventArgs> subscribe)
        {
            if (type == GameEventType.Update)
            {
                update.Merge(subscribe);
                return;
            }

            if (type == GameEventType.FixedUpdate)
            {
                fixedupdate.Merge(subscribe);
                return;
            }

            if (type == GameEventType.LateUpdate)
            {
                lateupdate.Merge(subscribe);
                return;
            }

            if (!map.TryGetValue(type, out ISubscribeHandle<UnityEventArgs> handle))
            {
                map.Add(type, subscribe);
                return;
            }

            handle.Merge(subscribe);
        }

        public void Unsubscribe(GameEventType type, ISubscribeHandle<UnityEventArgs> subscribe)
        {
            if (type == GameEventType.Update)
            {
                update.Unmerge(subscribe);
                return;
            }

            if (type == GameEventType.FixedUpdate)
            {
                fixedupdate.Unmerge(subscribe);
                return;
            }

            if (type == GameEventType.LateUpdate)
            {
                lateupdate.Unmerge(subscribe);
                return;
            }

            if (!map.TryGetValue(type, out ISubscribeHandle<UnityEventArgs> handle))
            {
                return;
            }

            handle.Unmerge(subscribe);
        }

        private void Execute(GameEventType eventType, object data = null)
        {
            if (!map.TryGetValue(eventType, out ISubscribeHandle<UnityEventArgs> subscrbe))
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
            update?.Execute(default);
        }

        private void FixedUpdate()
        {
            fixedupdate?.Execute(default);
        }

        private void LateUpdate()
        {
            lateupdate?.Execute(default);
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