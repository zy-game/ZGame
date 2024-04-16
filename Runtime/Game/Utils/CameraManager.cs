using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace ZGame.Game
{
    class CameraManager : IReference
    {
        class CameraItem : IReference
        {
            public int sort;
            public Camera camera;


            public void Release()
            {
                GameObject.DestroyImmediate(camera.gameObject);
            }

            public static CameraItem Create(string cameraName, int sort, params string[] layers)
            {
                CameraItem item = RefPooled.Spawner<CameraItem>();
                item.camera = new GameObject(cameraName).AddComponent<Camera>();
                item.camera.allowMSAA = false;
                if (item.camera.TryGetComponent<UniversalAdditionalCameraData>(out UniversalAdditionalCameraData universal) is false)
                {
                    universal = item.camera.gameObject.AddComponent<UniversalAdditionalCameraData>();
                }

                universal.renderShadows = false;
                universal.renderType = CameraRenderType.Overlay;
                item.camera.cullingMask = LayerMask.GetMask(layers);
                universal.volumeLayerMask = item.camera.cullingMask;
                return item;
            }
        }

        private List<CameraItem> items = new List<CameraItem>();
        public Camera main { get; private set; }
        public UniversalAdditionalCameraData cameraData { get; private set; }

        public void Release()
        {
            items.ForEach(RefPooled.Release);
            items.Clear();
            GameObject.DestroyImmediate(main.gameObject);
            main = null;
            cameraData = null;
        }

        public void LateUpdate()
        {
            
        }

        /// <summary>
        /// 设置子摄像机
        /// </summary>
        /// <param name="cameraName"></param>
        /// <param name="sort"></param>
        /// <param name="layers"></param>
        /// <returns></returns>
        public Camera SetSubCamera(string cameraName, int sort, params string[] layers)
        {
            CameraItem temp = CameraItem.Create(cameraName, sort, layers);
            items.Add(temp);
            items.Sort((x, y) => x.sort.CompareTo(y.sort));
            cameraData.cameraStack.Clear();
            foreach (var item in items)
            {
                cameraData.cameraStack.Add(item.camera);
            }

            return temp.camera;
        }

        /// <summary>
        /// 设置子摄像机位置和旋转
        /// </summary>
        /// <param name="cameraName"></param>
        /// <param name="position"></param>
        /// <param name="rotation"></param>
        public void SetSubCameraPositionAndRotation(string cameraName, Vector3 position, Quaternion rotation)
        {
            Camera main = GetSubCamera(cameraName);
            if (main is null)
            {
                return;
            }

            main.transform.position = position;
            main.transform.rotation = rotation;
        }

        /// <summary>
        /// 设置指定的相机跟随目标
        /// </summary>
        /// <param name="name"></param>
        /// <param name="target"></param>
        public void SetFollowTarget(string cameraName, Transform target)
        {
        }

        /// <summary>
        /// 设置指定的相机注视目标
        /// </summary>
        /// <param name="cameraName"></param>
        /// <param name="transform"></param>
        public void SetLockAtTarget(string cameraName, Transform transform)
        {
        }

        /// <summary>
        /// 获取子相机
        /// </summary>
        /// <param name="cameraName"></param>
        /// <returns></returns>
        public Camera GetSubCamera(string cameraName)
        {
            CameraItem item = items.FirstOrDefault(x => x.camera.name == cameraName);
            if (item is null || item.camera is null)
            {
                return default;
            }

            return item.camera;
        }

        public static CameraManager Create(string name)
        {
            CameraManager manager = RefPooled.Spawner<CameraManager>();
            manager.main = new GameObject(name).AddComponent<Camera>();
            manager.cameraData = manager.main.gameObject.AddComponent<UniversalAdditionalCameraData>();
            manager.cameraData.renderShadows = false;
            manager.cameraData.renderType = CameraRenderType.Base;
            manager.cameraData.volumeLayerMask = 0;
            manager.main.cullingMask = 0;
            manager.main.allowMSAA = false;
            return manager;
        }
    }
}