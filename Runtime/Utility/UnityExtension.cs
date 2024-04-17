using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
using ZGame.Game;

namespace ZGame
{
    public static partial class Extension
    {
        public static void SetIgnoreCertificate(this UnityWebRequest request)
        {
            if (request == null)
            {
                return;
            }

            request.certificateHandler = new CertificateController();
        }

        public static void SetRequestHeaderWithNotCors(this UnityWebRequest request, Dictionary<string, object> headers)
        {
            if (request == null)
            {
                return;
            }


            request.useHttpContinue = true;
            if (headers is null || headers.Count == 0)
            {
                return;
            }

            foreach (var header in headers)
            {
                request.SetRequestHeader(header.Key, header.Value.ToString());
            }
        }

        public static void SetRequestHeaderWithCors(this UnityWebRequest request, Dictionary<string, object> headers)
        {
            if (request == null)
            {
                return;
            }

            request.SetRequestHeader("Access-Control-Allow-Headers", "Content-Type");
            request.SetRequestHeader("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, OPTIONS");
            request.SetRequestHeader("Access-Control-Allow-Origin", "*");
            SetRequestHeaderWithNotCors(request, headers);
        }

        public static void SetRequestHeaderWithCors(this UnityWebRequest request)
        {
            if (request == null)
            {
                return;
            }

            SetRequestHeaderWithCors(request, null);
        }

        public static T GetResultData<T>(this UnityWebRequest request)
        {
            object _data = default;
            if (typeof(T) == typeof(string))
            {
                _data = request.downloadHandler.text;
            }
            else if (typeof(T) == typeof(byte[]))
            {
                _data = request.downloadHandler.data;
            }
            else if (typeof(T) is JObject)
            {
                _data = JObject.Parse(request.downloadHandler.text);
            }
            else
            {
                _data = JsonConvert.DeserializeObject<T>(request.downloadHandler.text);
            }

            return (T)_data;
        }

        public static void SetBoxCollider(this GameObject gameObject, Vector3 center, Vector3 size, bool isTrigger)
        {
            BoxCollider collider = gameObject.AddComponent<BoxCollider>();
            collider.isTrigger = isTrigger;
            collider.center = center;
            collider.size = size;
        }

        public static void SetBoxCollider2D(this GameObject gameObject, Vector3 center, Vector3 size, bool isTrigger)
        {
            BoxCollider2D collider = gameObject.AddComponent<BoxCollider2D>();
            collider.isTrigger = isTrigger;
            collider.size = size;
        }

        /// <summary>
        /// 注册物体碰撞进入事件
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="callback"></param>
        /// <param name="userData"></param>
        public static void SubscribeColliderEntryEvent(this GameObject gameObject, UnityAction<object> callback, object userData)
        {
            if (gameObject == null)
            {
                return;
            }

            gameObject.AddComponent<GameObjectHandle>().SubscribeColliderEntryEvent(callback, userData);
        }

        /// <summary>
        /// 取消注册物体碰撞进入事件
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="callback"></param>
        public static void UnsubscribeColliderEntryEvent(this GameObject gameObject, UnityAction<object> callback)
        {
            if (gameObject == null)
            {
                return;
            }

            gameObject.GetComponent<GameObjectHandle>().UnsubscribeColliderEntryEvent(callback);
        }

        /// <summary>
        /// 注册物体碰撞持续事件
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="callback"></param>
        /// <param name="userData"></param>
        public static void SubscribeColliderStayEvent(this GameObject gameObject, UnityAction<object> callback, object userData)
        {
            if (gameObject == null)
            {
                return;
            }

            gameObject.AddComponent<GameObjectHandle>().SubscribeColliderStayEvent(callback, userData);
        }

        /// <summary>
        /// 取消注册物体碰撞持续事件
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="callback"></param>
        public static void UnsubscribeColliderStayEvent(this GameObject gameObject, UnityAction<object> callback)
        {
            if (gameObject == null)
            {
                return;
            }

            gameObject.GetComponent<GameObjectHandle>().UnsubscribeColliderStayEvent(callback);
        }

        /// <summary>
        /// 注册物体碰撞退出事件
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="callback"></param>
        /// <param name="userData"></param>
        public static void SubscribeColliderExitEvent(this GameObject gameObject, UnityAction<object> callback, object userData)
        {
            if (gameObject == null)
            {
                return;
            }

            gameObject.AddComponent<GameObjectHandle>().SubscribeColliderExitEvent(callback, userData);
        }

        /// <summary>
        /// 取消注册物体碰撞退出事件
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="callback"></param>
        public static void UnsubscribeColliderExitEvent(this GameObject gameObject, UnityAction<object> callback)
        {
            if (gameObject == null)
            {
                return;
            }

            gameObject.GetComponent<GameObjectHandle>().UnsubscribeColliderExitEvent(callback);
        }


        public static bool IsPointerOverGameObject()
        {
#if UNITY_EDITOR
            return EventSystem.current.IsPointerOverGameObject();
#else
            if (Input.touchCount == 0)
            {
                return EventSystem.current.IsPointerOverGameObject();
            }
            return EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId);
#endif
        }

        public static void Active(params GameObject[] gameObjects)
        {
            for (int i = 0; i < gameObjects.Length; i++)
            {
                gameObjects[i].SetActive(true);
            }
        }

        public static void Inactive(params GameObject[] gameObjects)
        {
            for (int i = 0; i < gameObjects.Length; i++)
            {
                gameObjects[i].SetActive(false);
            }
        }

        public static void SetActive(bool state, params GameObject[] gameObjects)
        {
            if (state)
            {
                Active(gameObjects);
            }
            else
            {
                Inactive(gameObjects);
            }
        }

        /// <summary>
        /// 注册有事物体删除事件
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="action"></param>
        public static void SubscribeDestroyEvent(this GameObject gameObject, UnityAction action)
        {
            if (gameObject == null)
            {
                return;
            }

            GameObjectHandle bevaviour = gameObject.GetComponent<GameObjectHandle>();
            if (bevaviour == null)
            {
                bevaviour = gameObject.AddComponent<GameObjectHandle>();
            }

            bevaviour.SubscribeDestroyEvent(action);
        }

        /// <summary>
        /// 取消游戏物体删除事件
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="action"></param>
        public static void UnsubscribeDestroyEvent(this GameObject gameObject, UnityAction action)
        {
            if (gameObject == null)
            {
                return;
            }

            GameObjectHandle bevaviour = gameObject.GetComponent<GameObjectHandle>();
            if (bevaviour == null)
            {
                return;
            }

            bevaviour.UnsubscribeDestroyEvent(action);
        }

        public static void SetParent(this GameObject gameObject, Transform parent)
        {
            gameObject.transform.SetParent(parent);
        }

        public static void SetParent(this GameObject gameObject, Transform parent, Vector3 position, Vector3 rotation, Vector3 scale)
        {
            gameObject.transform.SetParent(parent);
            gameObject.transform.localPosition = position;
            gameObject.transform.localRotation = Quaternion.Euler(rotation);
            gameObject.transform.localScale = scale;
        }

        public static string ToBase64String(this AudioClip clip)
        {
            int position = clip.samples;
            float[] soundata = new float[position * clip.channels];
            clip.GetData(soundata, 0);
            int rescaleFactor = 32767;
            byte[] outData = new byte[soundata.Length * 2];
            for (int i = 0; i < soundata.Length; i++)
            {
                short temshort = (short)(soundata[i] * rescaleFactor);
                byte[] temdata = BitConverter.GetBytes(temshort);
                outData[i * 2] = temdata[0];
                outData[i * 2 + 1] = temdata[1];
            }

            return Convert.ToBase64String(outData);
        }

        public static Texture2D Screenshot(this Camera camera, int width, int height, GameObject gameObject)
        {
            Vector3 position = camera.transform.position;
            Quaternion rotation = camera.transform.rotation;
            Vector3 scale = camera.transform.localScale;
            float view = camera.fieldOfView;

            camera.SetupCameraFocusAndMaximizeTargetObject(gameObject);
            RenderTexture renderTexture = new RenderTexture(width, height, 0, RenderTextureFormat.Default);
            camera.targetTexture = renderTexture;
            RenderTexture.active = camera.targetTexture;
            camera.Render();
            Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            texture.name = gameObject.name.Replace("(Clone)", "");
            texture.Apply();
            RenderTexture.active = null;
            camera.targetTexture = null;
            camera.transform.position = position;
            camera.transform.rotation = rotation;
            camera.transform.localScale = scale;
            camera.fieldOfView = view;
            return texture;
        }

        public static void SetupCameraFocusAndMaximizeTargetObject(this Camera camera, GameObject gameObject)
        {
            if (camera == null || gameObject == null)
            {
                return;
            }

            Bounds meshBound = GetGameObjectBoundSize(gameObject); //renderer.sharedMesh.bounds;
            float max = Mathf.Max(meshBound.extents.x, meshBound.extents.y, meshBound.extents.z);
            camera.transform.position = new Vector3(0, meshBound.center.y, camera.transform.position.z);
            float distance = Vector3.Distance(camera.transform.position, gameObject.transform.position);
            camera.fieldOfView = 2.0f * Mathf.Atan(max / distance) * Mathf.Rad2Deg;
        }

        /// <summary>
        /// 获取物体包围盒
        /// </summary>
        /// <param name="obj">父物体</param>
        /// <returns>物体包围盒</returns>
        public static Bounds GetGameObjectBoundSize(this GameObject obj)
        {
            var bounds = new Bounds();
            if (obj == null)
            {
                return bounds;
            }

            var renders = obj.GetComponentsInChildren<SkinnedMeshRenderer>();
            var filter = obj.GetComponentsInChildren<MeshFilter>();
            var boundscenter = Vector3.zero;
            int count = 0;
            if (filter is not null && filter.Length > 0)
            {
                foreach (var VARIABLE in filter)
                {
                    // VARIABLE.sharedMesh.RecalculateBounds();
                    boundscenter += VARIABLE.sharedMesh.bounds.center;
                    CoreAPI.Logger.Log(VARIABLE.sharedMesh.bounds);
                    count++;
                }
            }

            if (renders is not null && renders.Length > 0)
            {
                foreach (var item in renders)
                {
                    item.sharedMesh.RecalculateBounds();
                    boundscenter += item.bounds.center;
                    count++;
                }
            }

            if (count > 0)
            {
                boundscenter /= count;
            }

            bounds = new Bounds(boundscenter, Vector3.zero);
            foreach (var item in renders)
            {
                bounds.Encapsulate(item.bounds);
            }

            foreach (var item in filter)
            {
                bounds.Encapsulate(item.sharedMesh.bounds);
            }

            return bounds;
        }

        class GameObjectHandle : MonoBehaviour
        {
            private object userData;
            private UnityEvent<object> entry = new();
            private UnityEvent<object> stay = new();
            private UnityEvent<object> exit = new();
            private UnityEvent _destroy = new();

            public void SubscribeDestroyEvent(UnityAction destroy)
            {
                _destroy.AddListener(destroy);
            }

            public void UnsubscribeDestroyEvent(UnityAction destroy)
            {
                _destroy.RemoveListener(destroy);
            }

            public void SubscribeColliderEntryEvent(UnityAction<object> collision, object userData)
            {
                this.entry.AddListener(collision);
                this.userData = userData;
            }

            public void UnsubscribeColliderEntryEvent(UnityAction<object> collision)
            {
                this.entry.RemoveListener(collision);
            }

            public void SubscribeColliderStayEvent(UnityAction<object> collision, object userData)
            {
                this.stay.AddListener(collision);
                this.userData = userData;
            }

            public void UnsubscribeColliderStayEvent(UnityAction<object> collision)
            {
                this.stay = null;
            }

            public void SubscribeColliderExitEvent(UnityAction<object> collision, object userData)
            {
                this.exit.AddListener(collision);
                this.userData = userData;
            }

            public void UnsubscribeColliderExitEvent(UnityAction<object> collision)
            {
                this.exit = null;
            }

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

                this.entry?.Invoke(userData);
            }

            private void OnStay(GameObject target)
            {
                if (target == null)
                {
                    return;
                }

                stay?.Invoke(userData);
            }

            private void OnExit(GameObject target)
            {
                if (target == null)
                {
                    return;
                }

                exit?.Invoke(userData);
            }

            void OnDestroy()
            {
                _destroy?.Invoke();
            }
        }

        class CertificateController : CertificateHandler
        {
            protected override bool ValidateCertificate(byte[] certificateData)
            {
                return true;
            }
        }
    }
}