using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ZEngine
{
    public sealed class UnityEventArgs : GameEventArgs<UnityEventArgs>
    {
    }

    class MonoEventListener : MonoBehaviour
    {
        private void Awake()
        {
        }

        private void Start()
        {
        }

        private void Update()
        {
        }

        private void FixedUpdate()
        {
        }

        private void LateUpdate()
        {
        }

        private void OnEnable()
        {
        }

        private void OnDisable()
        {
        }

        private void OnDestroy()
        {
        }

        private void OnGUI()
        {
        }

        private void OnApplicationFocus(bool hasFocus)
        {
        }

        private void OnApplicationPause(bool pauseStatus)
        {
        }

        private void OnBecameInvisible()
        {
        }

        private void OnBecameVisible()
        {
        }


        private void OnCollisionEnter(Collision other)
        {
        }

        private void OnCollisionEnter2D(Collision2D other)
        {
        }

        private void OnCollisionExit(Collision other)
        {
        }

        private void OnCollisionExit2D(Collision2D other)
        {
        }

        private void OnCollisionStay(Collision other)
        {
        }

        private void OnCollisionStay2D(Collision2D other)
        {
        }


        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
        }

        private void OnDrawGizmos()
        {
        }

        private void OnDrawGizmosSelected()
        {
        }

        private void OnJointBreak(float breakForce)
        {
        }

        private void OnJointBreak2D(Joint2D brokenJoint)
        {
        }

        private void OnMouseDown()
        {
        }

        private void OnMouseDrag()
        {
        }

        private void OnMouseEnter()
        {
        }

        private void OnMouseExit()
        {
        }

        private void OnMouseOver()
        {
        }

        private void OnMouseUp()
        {
        }

        private void OnMouseUpAsButton()
        {
        }

        private void OnTriggerEnter(Collider other)
        {
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
        }

        private void OnTriggerExit(Collider other)
        {
        }

        private void OnTriggerExit2D(Collider2D other)
        {
        }

        private void OnTriggerStay(Collider other)
        {
        }

        private void OnTriggerStay2D(Collider2D other)
        {
        }

        private void OnValidate()
        {
        }

        private void OnWillRenderObject()
        {
        }


        public void OnApplicationQuit()
        {
            Engine.Console.Log("Mono Event", GameEventType.OnApplicationQuit);
            IEnumerable<GameEventSubscrbe<UnityEventArgs>> list = UnityEventArgs.subscribes.Where(x => x.type == GameEventType.OnApplicationQuit);
            foreach (var VARIABLE in list)
            {
                VARIABLE.Execute(new UnityEventArgs());
            }
        }
    }
}