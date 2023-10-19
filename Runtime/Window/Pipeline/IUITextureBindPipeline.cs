using System;
using UnityEngine;
using UnityEngine.UI;
using ZEngine.Resource;

namespace ZEngine.Window
{
    public interface IUITextureBindPipeline : IUIComponentBindPipeline, IValueBindPipeline<Texture>
    {
        public static IUITextureBindPipeline Create(UIWindow window, string path)
        {
            GameObject gameObject = window.GetChild(path);
            if (gameObject == null)
            {
                return default;
            }

            RawImage rawImage = gameObject.GetComponent<RawImage>();
            if (rawImage == null)
            {
                return default;
            }

            TextureBindPipelineHandle textureBindPipelineHandle = new TextureBindPipelineHandle();
            textureBindPipelineHandle.window = window;
            textureBindPipelineHandle.path = path;
            textureBindPipelineHandle.name = gameObject.name;
            textureBindPipelineHandle.gameObject = gameObject;
            textureBindPipelineHandle.image = rawImage;
            textureBindPipelineHandle.Active();
            return textureBindPipelineHandle;
        }

        public static IUITextureBindPipeline Create(UIWindow window, string path, Texture texture)
        {
            IUITextureBindPipeline textureBindPipeline = Create(window, path);
            if (textureBindPipeline is null)
            {
                return default;
            }

            textureBindPipeline.SetValueWithoutNotify(texture);
            return textureBindPipeline;
        }

        public static IUITextureBindPipeline Create(UIWindow window, string path, string texture)
        {
            IRequestResourceObjectResult requestResourceObjectResult = ZGame.Resource.LoadAsset(texture);
            if (requestResourceObjectResult.status is not Status.Success)
            {
                return default;
            }

            return Create(window, path, requestResourceObjectResult.GetObject<Texture2D>());
        }


        class TextureBindPipelineHandle : IUITextureBindPipeline
        {
            public string name { get; set; }
            public bool actived { get; set; }
            public string path { get; set; }
            public UIWindow window { get; set; }
            public GameObject gameObject { get; set; }
            public Texture value { get; set; }
            public RawImage image { get; set; }
            public event Action<Texture> Callback;

            public void Dispose()
            {
                ZGame.Resource.Release(value);
                this.name = String.Empty;
                this.path = String.Empty;
                this.window = null;
                this.gameObject = null;
                this.value = null;
                this.image = null;
                this.actived = false;
            }

            public void Active()
            {
                this.actived = true;
            }

            public void Inactive()
            {
                this.actived = false;
            }

            public void Enable()
            {
                if (gameObject == null)
                {
                    return;
                }

                this.gameObject.SetActive(true);
                Active();
            }

            public void Disable()
            {
                if (this.gameObject == null)
                {
                    return;
                }

                this.gameObject.SetActive(false);
                Inactive();
            }

            public void SetValueWithoutNotify(Texture value)
            {
                if (gameObject == null)
                {
                    return;
                }

                if (image == null)
                {
                    return;
                }

                this.value = (Texture)value;
                image.texture = (Texture)value;
            }

            public void SetValue(Texture value)
            {
                SetValueWithoutNotify(value);
                Invoke(value);
            }

            public void AddListener(Action<Texture> callback)
            {
                this.Callback += callback;
            }

            public void RemoveListener(Action<Texture> callback)
            {
                this.Callback -= callback;
            }

            public void Invoke(Texture args)
            {
                if (actived is false)
                {
                    return;
                }

                this.Callback?.Invoke(args);
            }
        }
    }
}