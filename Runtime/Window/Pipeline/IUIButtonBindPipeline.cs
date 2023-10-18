using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ZEngine.Window
{
    public interface IUIButtonBindPipeline : IEventBindPipeline<IUIButtonBindPipeline>, IUIComponentBindPipeline
    {
        public static IUIButtonBindPipeline Create(UIWindow window, string path)
        {
            GameObject gameObject = window.GetChild(path);
            if (gameObject == null)
            {
                return default;
            }

            Button btn = gameObject.GetComponent<Button>();
            if (btn == null)
            {
                return default;
            }

            ButtonBindPipelineHandle buttonBindPipelineHandle = new ButtonBindPipelineHandle();
            buttonBindPipelineHandle.window = window;
            buttonBindPipelineHandle.path = path;
            buttonBindPipelineHandle.gameObject = gameObject;
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => buttonBindPipelineHandle.Invoke(default));
            buttonBindPipelineHandle.Active();
            return buttonBindPipelineHandle;
        }

        class ButtonBindPipelineHandle : IUIButtonBindPipeline
        {
            public string path { get; set; }
            public string name { get; set; }
            public UIWindow window { get; set; }
            public GameObject gameObject { get; set; }
            public bool actived { get; set; }

            private event Action<IUIButtonBindPipeline, object> callback;

            public void Enable()
            {
                if (this.gameObject == null)
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

            public void SetValueWithoutNotify(object value)
            {
            }

            public void Active()
            {
                this.actived = true;
            }

            public void Inactive()
            {
                this.actived = false;
            }

            public void AddListener(Action<IUIButtonBindPipeline, object> callback)
            {
                this.callback += callback;
            }

            public void RemoveListener(Action<IUIButtonBindPipeline, object> callback)
            {
                this.callback -= callback;
            }

            public void Invoke(object args)
            {
                if (this.actived is false)
                {
                    return;
                }

                this.callback?.Invoke(this, args);
            }

            public void Dispose()
            {
                this.actived = false;
                this.callback = null;
                this.gameObject = null;
                this.name = String.Empty;
                this.path = String.Empty;
                this.window = null;
            }
        }
    }
}