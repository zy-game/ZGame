using UnityEngine;
using UnityEngine.Events;

namespace ZEngine.Resource
{
    class ObserverGameObjectDestroy : MonoBehaviour
    {
        public UnityEvent subscribe = new UnityEvent();

        private void OnDestroy()
        {
            subscribe.Invoke();
        }
    }
}