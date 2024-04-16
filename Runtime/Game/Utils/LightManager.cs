using UnityEngine;

namespace ZGame.Game
{
    class LightManager : IReference
    {
        public Light main { get; private set; }
        public Gradient gradient { get; private set; }

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
            LightManager manager = RefPooled.Spawner<LightManager>();
            manager.main = new GameObject(name + "_MainLight").AddComponent<Light>();
            manager.main.type = LightType.Directional;
            manager.main.transform.rotation = Quaternion.Euler(50, -30, 0);
            manager.main.intensity = 1;
            manager.main.renderMode = LightRenderMode.Auto;
            manager.main.shadows = LightShadows.None;
            manager.main.color = color;
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
    }
}