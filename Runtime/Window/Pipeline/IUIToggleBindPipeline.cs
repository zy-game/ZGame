using System;
using UnityEngine;

namespace ZEngine.Window
{
    public interface IUIToggleBindPipeline : IUIComponentBindPipeline
    {
        public static IUIToggleBindPipeline Create(UIWindow window, string path)
        {
            return new ToggleBindPipelineHandle(window, path);
        }

        class ToggleBindPipelineHandle : IUIToggleBindPipeline
        {
            public string path { get; }
            public string name { get; }
            public object value { get; }
            public UIWindow window { get; }
            public GameObject gameObject { get; }

            public ToggleBindPipelineHandle(UIWindow window, string path)
            {
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

            public void Dispose()
            {
                throw new NotImplementedException();
            }
        }
    }
}