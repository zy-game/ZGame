using System;
using UnityEngine;
using UnityEngine.Events;

namespace ZGame
{
    public static partial class Extension
    {
        public static void OnListenDestroyEvent(this GameObject gameObject, UnityAction action)
        {
            BehaviourScriptable.instance.ListenerDestroy(gameObject, action);
        }

        public static void SetParent(this GameObject gameObject, Transform parent)
        {
            gameObject.transform.SetParent(parent);
        }

        public static void SetParent(this GameObject gameObject, Transform parent, Vector3 position, Vector3 rotation, Vector3 scale)
        {
            gameObject.transform.SetParent(parent);
            gameObject.transform.position = position;
            gameObject.transform.rotation = Quaternion.Euler(rotation);
            gameObject.transform.localScale = scale;
            RectTransform rectTransform = default;
            if ((rectTransform = gameObject.GetComponent<RectTransform>()) == null)
            {
                return;
            }

            rectTransform.anchoredPosition = position;
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
                    Debug.Log(VARIABLE.sharedMesh.bounds);
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
    }
}