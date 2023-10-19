using System;
using UnityEngine;
using UnityEngine.UI;

namespace ZEngine.Window
{
    public interface IUIToggleBindPipeline : IUIComponentBindPipeline, IValueBindPipeline<bool>
    {
        public static IUIToggleBindPipeline Create(UIWindow window, string path)
        {
            GameObject gameObject = window.GetChild(path);
            if (gameObject == null)
            {
                return default;
            }

            Toggle toggle = gameObject.GetComponent<Toggle>();
            if (toggle == null)
            {
                return default;
            }

            ToggleBindPipelineHandle toggleBindPipelineHandle = new ToggleBindPipelineHandle();
            toggleBindPipelineHandle.window = window;
            toggleBindPipelineHandle.path = path;
            toggleBindPipelineHandle.name = gameObject.name;
            toggleBindPipelineHandle.gameObject = gameObject;
            toggleBindPipelineHandle.toggle = toggle;
            toggleBindPipelineHandle.Active();
            toggle.onValueChanged.RemoveAllListeners();
            toggle.onValueChanged.AddListener(args => toggleBindPipelineHandle.SetValue(args));
            return toggleBindPipelineHandle;
        }

        public static IUIToggleBindPipeline Create(UIWindow window, string path, Action<bool> callback)
        {
            IUIToggleBindPipeline toggleBindPipeline = Create(window, path);
            if (toggleBindPipeline is not null)
            {
                toggleBindPipeline.AddListener(callback);
            }

            return toggleBindPipeline;
        }

        class ToggleBindPipelineHandle : IUIToggleBindPipeline
        {
            public string path { get; set; }
            public string name { get; set; }
            public bool actived { get; set; }
            public bool value { get; set; }
            public UIWindow window { get; set; }
            public GameObject gameObject { get; set; }
            public Toggle toggle { get; set; }
            public event Action<bool> Callback;

            public void SetValue(bool value)
            {
                SetValueWithoutNotify(value);
                Invoke(value);
            }

            public void Enable()
            {
                if (gameObject == null)
                {
                    return;
                }

                gameObject.SetActive(true);
                Active();
            }

            public void Disable()
            {
                if (gameObject == null)
                {
                    return;
                }

                gameObject.SetActive(false);
                Inactive();
            }

            public void SetValueWithoutNotify(bool value)
            {
                if (gameObject == null)
                {
                    return;
                }

                if (toggle == null)
                {
                    return;
                }

                this.value = (bool)value;
                toggle.isOn = this.value;
            }

            public void Active()
            {
                this.actived = true;
            }

            public void Inactive()
            {
                this.actived = false;
            }

            public void Dispose()
            {
                this.name = String.Empty;
                this.path = String.Empty;
                this.window = null;
                this.gameObject = null;
                this.toggle = null;
                this.value = false;
                this.actived = false;
            }

            public void AddListener(Action<bool> callback)
            {
                this.Callback += callback;
            }

            public void RemoveListener(Action<bool> callback)
            {
                this.Callback -= callback;
            }

            public void Invoke(bool args)
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