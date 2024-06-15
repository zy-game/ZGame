using UnityEngine;

namespace ZGame.Game
{
    class SkyboxManager : IReference
    {
        private Camera camera;
        public Skybox box { get; private set; }
        public Material material { get; private set; }

        public void Release()
        {
            box.enabled = false;
            box.material = null;
            camera.clearFlags = CameraClearFlags.Color;
            camera.backgroundColor = Color.clear;
        }

        public static SkyboxManager Create(string name, Camera transform)
        {
            SkyboxManager manager = RefPooled.Alloc<SkyboxManager>();
            if (transform.gameObject.TryGetComponent<Skybox>(out var box) is false)
            {
                box = transform.gameObject.AddComponent<Skybox>();
            }

            manager.box = box;
            manager.camera = transform;
            return manager;
        }

        public void SetSkybox(Material material1)
        {
            box.material = material;
            camera.clearFlags = CameraClearFlags.Skybox;
        }
    }
}