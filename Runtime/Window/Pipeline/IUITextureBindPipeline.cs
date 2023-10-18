using System;
using UnityEngine;

namespace ZEngine.Window
{
    public interface IUITextureBindPipeline : IUIComponentBindPipeline
    {
        public static IUITextureBindPipeline Create(UIWindow window, string path)
        {
            return new TextureBindPipelineHandle(window, path);
        }

        class TextureBindPipelineHandle : IUITextureBindPipeline
        {
            public string path { get; }
            public string name { get; }
            public object value { get; }
            public UIWindow window { get; }
            public GameObject gameObject { get; }

            public TextureBindPipelineHandle(UIWindow window, string path)
            {
            }

            public void Dispose()
            {
                throw new NotImplementedException();
            }

            public void Active()
            {
                throw new NotImplementedException();
            }

            public void Inactive()
            {
                throw new NotImplementedException();
            }

            public void OnChange(object args)
            {
                throw new NotImplementedException();
            }
        }
    }
}