using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace ZGame.Game
{
    public class CameraManager : GameManager
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
                CameraItem item = RefPooled.Alloc<CameraItem>();
                item.sort = sort;
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

        private LightManager _lightManager;
        private SkyboxManager _skyboxManager;
        private List<CameraItem> items = new List<CameraItem>();
        public Camera main { get; private set; }

        public Camera uiCamera { get; private set; }

        public Light mainLight => _lightManager.main;
        public UniversalAdditionalCameraData cameraData { get; private set; }

        public override void OnAwake(params object[] args)
        {
            main = new GameObject(AppCore.name).AddComponent<Camera>();
            cameraData = main.gameObject.AddComponent<UniversalAdditionalCameraData>();
            cameraData.renderShadows = false;
            cameraData.renderType = CameraRenderType.Base;
            cameraData.volumeLayerMask = 0;
            main.cullingMask = 0;
            main.allowMSAA = false;


            uiCamera = SetSubCamera("UICamera", false, Int32.MaxValue, "UI");
            uiCamera.orthographic = true;
            _lightManager = LightManager.Create(AppCore.name, Color.white);
            _skyboxManager = SkyboxManager.Create(AppCore.name, main);
            GameObject.DontDestroyOnLoad(main.gameObject);
            GameObject.DontDestroyOnLoad(uiCamera.gameObject);
            GameObject.DontDestroyOnLoad(_lightManager.main.gameObject);
        }

        public void Release()
        {
            items.ForEach(RefPooled.Free);
            items.Clear();
            GameObject.DestroyImmediate(main.gameObject);
            main = null;
            cameraData = null;
        }

        /// <summary>
        /// 设置子摄像机
        /// </summary>
        /// <param name="cameraName"></param>
        /// <param name="sort"></param>
        /// <param name="layers"></param>
        /// <returns></returns>
        public Camera SetSubCamera(string cameraName, bool isShaddow, int sort, params string[] layers)
        {
            CameraItem temp = CameraItem.Create(cameraName, sort, layers);
            items.Add(temp);
            items.Sort((x, y) => x.sort.CompareTo(y.sort));
            cameraData.cameraStack.Clear();
            foreach (var item in items)
            {
                cameraData.cameraStack.Add(item.camera);
            }

            temp.camera.GetComponent<UniversalAdditionalCameraData>().renderShadows = isShaddow;
            return temp.camera;
        }

        /// <summary>
        /// 设置子摄像机位置和旋转
        /// </summary>
        /// <param name="cameraName"></param>
        /// <param name="position"></param>
        /// <param name="rotation"></param>
        public void SetSubCameraPositionAndRotation(string cameraName, Vector3 position, Vector3 rotation)
        {
            Camera main = GetSubCamera(cameraName);
            if (main is null)
            {
                return;
            }

            main.transform.position = position;
            main.transform.rotation = Quaternion.Euler(rotation);
        }

        class Follower : IReference
        {
            public Camera camera;
            public Transform target;
            public Vector3 offset;

            public void Release()
            {
            }
        }

        class Locker : IReference
        {
            public Camera camera;
            public Transform target;

            public void Release()
            {
            }
        }

        private List<Locker> lockers = new List<Locker>();
        private List<Follower> followers = new List<Follower>();

        protected override void LateUpdate()
        {
            for (int i = lockers.Count - 1; i >= 0; i--)
            {
                lockers[i].camera.transform.LookAt(lockers[i].target);
            }

            for (int i = followers.Count - 1; i >= 0; i--)
            {
                followers[i].camera.transform.position = Vector3.Lerp(followers[i].camera.transform.position, followers[i].target.position + followers[i].offset, 0.1f);
            }
        }

        /// <summary>
        /// 设置指定的相机跟随目标
        /// </summary>
        /// <param name="name"></param>
        /// <param name="target"></param>
        public void SetFollowTarget(string cameraName, Transform target)
        {
            Follower follower = RefPooled.Alloc<Follower>();
            follower.camera = GetSubCamera(cameraName);
            follower.target = target;
            follower.offset = follower.camera.transform.position - follower.target.position;
            followers.Add(follower);
        }

        /// <summary>
        /// 设置指定的相机注视目标
        /// </summary>
        /// <param name="cameraName"></param>
        /// <param name="transform"></param>
        public void SetLockAtTarget(string cameraName, Transform transform)
        {
            Locker locker = RefPooled.Alloc<Locker>();
            locker.camera = GetSubCamera(cameraName);
            locker.target = transform;
            lockers.Add(locker);
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


        /// <summary>
        /// 设置阳光颜色
        /// </summary>
        /// <param name="gradient"></param>
        public void SetSunshine(Gradient gradient)
        {
            _lightManager.SetSunshine(gradient);
        }

        /// <summary>
        /// 添加灯光
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="name"></param>
        /// <param name="color"></param>
        /// <param name="type"></param>
        /// <param name="intensity"></param>
        /// <param name="mode"></param>
        /// <param name="position"></param>
        /// <param name="rotation"></param>
        public void AddLight(Transform parent, string name, Color color, LightType type, float intensity, LightRenderMode mode)
        {
            _lightManager.AddLight(name, color, type, intensity, mode);
            _lightManager.SetLightPositionAndRotation(name, parent, Vector3.zero, Vector3.zero);
        }

        public void RemoveLight(string name)
        {
            _lightManager.RemoveLight(name);
        }

        /// <summary>
        /// 设置灯光位置和旋转
        /// </summary>
        /// <param name="name"></param>
        /// <param name="position"></param>
        /// <param name="rotation"></param>
        public void SetLightPositionAndRotation(string name, Transform parent, Vector3 position, Vector3 rotation)
        {
            _lightManager.SetLightPositionAndRotation(name, parent, position, rotation);
        }

        /// <summary>
        /// 设置灯光强度
        /// </summary>
        /// <param name="name"></param>
        /// <param name="intensity"></param>
        public void SetLightIntensity(string name, float intensity)
        {
            _lightManager.SetLightIntensity(name, intensity);
        }

        /// <summary>
        /// 设置灯光颜色
        /// </summary>
        /// <param name="name"></param>
        /// <param name="color"></param>
        public void SetLightColor(string name, Color color)
        {
            _lightManager.SetLightColor(name, color);
        }

        /// <summary>
        /// 设置灯光渲染模式
        /// </summary>
        /// <param name="name"></param>
        /// <param name="mode"></param>
        public void SetLightRenderMode(string name, LightRenderMode mode)
        {
            _lightManager.SetLightRenderMode(name, mode);
        }

        /// <summary>
        /// 设置灯光类型
        /// </summary>
        /// <param name="name"></param>
        /// <param name="type"></param>
        public void SetLightType(string name, LightType type)
        {
            _lightManager.SetLightType(name, type);
        }

        /// <summary>
        /// 设置灯光范围
        /// </summary>
        /// <param name="name"></param>
        /// <param name="range"></param>
        public void SetLightRange(string name, float range)
        {
            _lightManager.SetLightRange(name, range);
        }

        /// <summary>
        /// 设置角度
        /// </summary>
        /// <param name="name"></param>
        /// <param name="angle"></param>
        public void SetLightSpotAngle(string name, float angle)
        {
            _lightManager.SetLightSpotAngle(name, angle);
        }

        /// <summary>
        /// 设置阴影强度
        /// </summary>
        /// <param name="name"></param>
        /// <param name="strength"></param>
        public void SetLightShadowStrength(string name, float strength)
        {
            _lightManager.SetLightShadowStrength(name, strength);
        }

        /// <summary>
        /// 设置阴影渲染模式
        /// </summary>
        /// <param name="name"></param>
        /// <param name="state"></param>
        public void SetLightShadow(string name, LightShadows state)
        {
            _lightManager.SetLightShadow(name, state);
        }

        /// <summary>
        /// 设置天空盒
        /// </summary>
        /// <param name="material"></param>
        public void SetSkybox(Material material)
        {
            _skyboxManager.SetSkybox(material);
        }

        /// <summary>
        /// 关闭天空盒
        /// </summary>
        public void CloseSkybox()
        {
            _skyboxManager.Release();
        }
    }
}