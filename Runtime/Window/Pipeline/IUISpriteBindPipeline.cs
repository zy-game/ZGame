using System;
using UnityEngine;
using UnityEngine.UI;
using ZEngine.Resource;

namespace ZEngine.Window
{
    public interface IUISpriteBindPipeline : IUIComponentBindPipeline, IValueBindPipeline<Sprite>
    {
        public static IUISpriteBindPipeline Create(UIWindow window, string path)
        {
            GameObject gameObject = window.GetChild(path);
            if (gameObject == null)
            {
                return default;
            }

            Image image = gameObject.GetComponent<Image>();
            if (image == null)
            {
                return default;
            }

            SpriteBindPipelineHandle spriteBindPipelineHandle = new SpriteBindPipelineHandle();
            spriteBindPipelineHandle.window = window;
            spriteBindPipelineHandle.path = path;
            spriteBindPipelineHandle.name = gameObject.name;
            spriteBindPipelineHandle.gameObject = gameObject;
            spriteBindPipelineHandle.image = image;
            spriteBindPipelineHandle.Active();
            return spriteBindPipelineHandle;
        }

        public static IUISpriteBindPipeline Create(UIWindow window, string path, string sprite)
        {
            IRequestResourceObjectResult requestResourceObjectResult = ZGame.Resource.LoadAsset(sprite);
            if (requestResourceObjectResult.status is not Status.Success)
            {
                return default;
            }

            return Create(window, path, requestResourceObjectResult.GetObject<Sprite>());
        }

        public static IUISpriteBindPipeline Create(UIWindow window, string path, Sprite sprite)
        {
            IUISpriteBindPipeline spriteBindPipeline = Create(window, path);
            if (spriteBindPipeline is null)
            {
                return default;
            }

            spriteBindPipeline.SetValue(sprite);
            return spriteBindPipeline;
        }

        class SpriteBindPipelineHandle : IUISpriteBindPipeline
        {
            public Sprite value { get; set; }
            public string path { get; set; }
            public string name { get; set; }
            public bool actived { get; set; }
            public UIWindow window { get; set; }
            public GameObject gameObject { get; set; }
            public Image image { get; set; }

            public void Active()
            {
                this.actived = true;
            }

            public void Inactive()
            {
                this.actived = false;
            }

            public void SetValue(Sprite value)
            {
                if (gameObject == null)
                {
                    return;
                }

                if (image == null)
                {
                    return;
                }

                if (this.value != null)
                {
                    ZGame.Resource.Release(this.value);
                }

                this.value = value;
                image.sprite = this.value;
            }

            public void Enable()
            {
                if (this.gameObject == null)
                {
                    return;
                }

                this.gameObject.SetActive(true);
            }

            public void Disable()
            {
                if (this.gameObject == null)
                {
                    return;
                }

                this.gameObject.SetActive(false);
            }

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
        }
    }
}