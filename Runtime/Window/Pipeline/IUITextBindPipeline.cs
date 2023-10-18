using System;
using UnityEngine;
using UnityEngine.UI;

namespace ZEngine.Window
{
    public interface IUITextBindPipeline : IUIComponentBindPipeline, IValueBindPipeline<object>
    {
        public static IUITextBindPipeline Create(UIWindow window, string path)
        {
            return new TextBindPipelineHandle(window, path);
        }

        class TextBindPipelineHandle : IUITextBindPipeline
        {
            public Text text { get; set; }
            public string path { get; set; }
            public string name { get; set; }
            public bool actived { get; }
            public object value { get; set; }
            public UIWindow window { get; set; }
            public GameObject gameObject { get; set; }

            public void Enable()
            {
                throw new NotImplementedException();
            }

            public void Disable()
            {
                throw new NotImplementedException();
            }

            public void SetValue(object value)
            {
                throw new NotImplementedException();
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