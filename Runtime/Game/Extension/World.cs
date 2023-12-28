using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

namespace ZGame.Game
{
    public class World
    {
        public Camera main { get; }
        private Skybox skybox;
        private List<Camera> subCameras = new();
        private UniversalAdditionalCameraData universalAdditionalCameraData;


        internal World(string name)
        {
            main = new GameObject(name).AddComponent<Camera>();
            universalAdditionalCameraData = main.gameObject.AddComponent<UniversalAdditionalCameraData>();
            universalAdditionalCameraData.renderShadows = false;
            universalAdditionalCameraData.renderType = CameraRenderType.Base;
            main.cullingMask = 0;
            universalAdditionalCameraData.volumeLayerMask = 0;
            main.allowMSAA = false;
            skybox = main.gameObject.AddComponent<Skybox>();
        }

        public void SetRenderLayer(params string[] layers)
        {
            if (layers.Length == 0)
            {
                return;
            }

            main.cullingMask = LayerMask.GetMask(layers);
            universalAdditionalCameraData.volumeLayerMask = LayerMask.GetMask(layers);
        }

        public void SetSkybox(Material material)
        {
            skybox.material = material;
            main.clearFlags = CameraClearFlags.Skybox;
        }

        public void CloseSkybox()
        {
            skybox.enabled = false;
            main.clearFlags = CameraClearFlags.Color;
            main.backgroundColor = Color.clear;
        }

        public void NewCamera(string name, int sort, RenderTexture target, params string[] tag)
        {
        }
    }
}