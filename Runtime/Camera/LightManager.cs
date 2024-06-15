using System.Collections.Generic;
using UnityEngine;

namespace ZGame.Game
{
    class LightManager : IReference
    {
        public Light main { get; private set; }
        public Gradient gradient { get; private set; }
        private List<Light> lights = new List<Light>();

        public void Release()
        {
            GameObject.DestroyImmediate(main?.gameObject);
            main = null;
            gradient = null;
        }

        public void Refresh(float hour)
        {
            if (gradient == null)
            {
                return;
            }

            main.color = gradient.Evaluate(hour / 24f);
        }

        public static LightManager Create(string name, Color color)
        {
            name = name + "_Light";
            LightManager manager = RefPooled.Alloc<LightManager>();
            manager.main = manager.AddLight(name, color, LightType.Directional, 1, LightRenderMode.Auto);
            manager.SetLightPositionAndRotation(name, null, Vector3.zero, new Vector3(50, -30, 0));
            return manager;
        }

        public static LightManager Create(string name, Gradient color)
        {
            LightManager manager = Create(name, color.Evaluate(0));
            manager.gradient = color;
            return manager;
        }

        public void SetSunshine(Gradient gradient1)
        {
            this.gradient = gradient1;
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
        /// <returns></returns>
        public Light AddLight(string name, Color color, LightType type, float intensity, LightRenderMode mode)
        {
            GameObject gameObject = new GameObject(name);
            Light light = gameObject.AddComponent<Light>();
            lights.Add(light);
            this.SetLightColor(name, color);
            this.SetLightIntensity(name, intensity);
            this.SetLightRenderMode(name, mode);
            this.SetLightType(name, type);
            return light;
        }

        /// <summary>
        /// 设置灯光位置和旋转
        /// </summary>
        /// <param name="name"></param>
        /// <param name="position"></param>
        /// <param name="rotation"></param>
        public void SetLightPositionAndRotation(string name, Transform parent, Vector3 position, Vector3 rotation)
        {
            Light gameObject = lights.Find(item => item.name == name);
            if (gameObject != null)
            {
                gameObject.transform.position = position;
                gameObject.transform.rotation = Quaternion.Euler(rotation);
            }
        }

        /// <summary>
        /// 设置灯光强度
        /// </summary>
        /// <param name="name"></param>
        /// <param name="intensity"></param>
        public void SetLightIntensity(string name, float intensity)
        {
            Light gameObject = lights.Find(item => item.name == name);
            if (gameObject != null)
            {
                gameObject.intensity = intensity;
            }
        }

        /// <summary>
        /// 设置灯光颜色
        /// </summary>
        /// <param name="name"></param>
        /// <param name="color"></param>
        public void SetLightColor(string name, Color color)
        {
            Light gameObject = lights.Find(item => item.name == name);
            if (gameObject != null)
            {
                gameObject.color = color;
            }
        }

        /// <summary>
        /// 设置灯光渲染模式
        /// </summary>
        /// <param name="name"></param>
        /// <param name="mode"></param>
        public void SetLightRenderMode(string name, LightRenderMode mode)
        {
            Light gameObject = lights.Find(item => item.name == name);
            if (gameObject != null)
            {
                gameObject.renderMode = mode;
            }
        }

        /// <summary>
        /// 设置灯光类型
        /// </summary>
        /// <param name="name"></param>
        /// <param name="type"></param>
        public void SetLightType(string name, LightType type)
        {
            Light gameObject = lights.Find(item => item.name == name);
            if (gameObject != null)
            {
                gameObject.type = type;
            }
        }

        /// <summary>
        /// 设置灯光范围
        /// </summary>
        /// <param name="name"></param>
        /// <param name="range"></param>
        public void SetLightRange(string name, float range)
        {
            Light gameObject = lights.Find(item => item.name == name);
            if (gameObject != null)
            {
                gameObject.range = range;
            }
        }

        /// <summary>
        /// 设置角度
        /// </summary>
        /// <param name="name"></param>
        /// <param name="angle"></param>
        public void SetLightSpotAngle(string name, float angle)
        {
            Light gameObject = lights.Find(item => item.name == name);
            if (gameObject != null)
            {
                gameObject.spotAngle = angle;
            }
        }

        /// <summary>
        /// 设置阴影强度
        /// </summary>
        /// <param name="name"></param>
        /// <param name="strength"></param>
        public void SetLightShadowStrength(string name, float strength)
        {
            Light gameObject = lights.Find(item => item.name == name);
            if (gameObject != null)
            {
                gameObject.shadowStrength = strength;
            }
        }

        /// <summary>
        /// 设置阴影渲染模式
        /// </summary>
        /// <param name="name"></param>
        /// <param name="state"></param>
        public void SetLightShadow(string name, LightShadows state)
        {
            Light gameObject = lights.Find(item => item.name == name);
            if (gameObject != null)
            {
                gameObject.shadows = state;
            }
        }

        /// <summary>
        /// 移除灯光
        /// </summary>
        /// <param name="name"></param>
        public void RemoveLight(string name)
        {
            lights.RemoveAll(item => item.name == name);
        }
    }
}