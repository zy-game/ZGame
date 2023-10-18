using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ZEngine.Window
{
    public interface IUIInputFieldBindPipeline : IUIComponentBindPipeline, IValueBindPipeline<string>, IEventBindPipeline<IUIInputFieldBindPipeline>
    {
        void SetTextWithoutNotify(string value);

        public static IUIInputFieldBindPipeline Create(UIWindow window, string path)
        {
            GameObject gameObject = window.GetChild(path);
            if (gameObject == null)
            {
                return default;
            }

            TMP_InputField tmpInputField = gameObject.GetComponent<TMP_InputField>();
            if (tmpInputField == null)
            {
                return default;
            }

            InputFieldBindPipelineHandle inputFieldBindPipelineHandle = new InputFieldBindPipelineHandle();
            inputFieldBindPipelineHandle.window = window;
            inputFieldBindPipelineHandle.path = path;
            inputFieldBindPipelineHandle.name = gameObject.name;
            inputFieldBindPipelineHandle.gameObject = gameObject;
            tmpInputField.onEndEdit.RemoveAllListeners();
            tmpInputField.onEndEdit.AddListener(args => inputFieldBindPipelineHandle.SetValue(args));
            inputFieldBindPipelineHandle.Active();
            return inputFieldBindPipelineHandle;
        }

        public static IUIInputFieldBindPipeline Create(UIWindow window, string path, string text, Action<IUIInputFieldBindPipeline, object> callback)
        {
            IUIInputFieldBindPipeline inputFieldBindPipeline = Create(window, path);
            inputFieldBindPipeline.SetTextWithoutNotify(text);
            inputFieldBindPipeline.AddListener(callback);
            return inputFieldBindPipeline;
        }

        class InputFieldBindPipelineHandle : IUIInputFieldBindPipeline
        {
            public string name { get; set; }
            public bool actived { get; set; }
            public string path { get; set; }
            public UIWindow window { get; set; }
            public GameObject gameObject { get; set; }
            public string value { get; set; }
            private event Action<IUIInputFieldBindPipeline, object> callback;

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

            public void AddListener(Action<IUIInputFieldBindPipeline, object> callback)
            {
                this.callback += callback;
            }

            public void RemoveListener(Action<IUIInputFieldBindPipeline, object> callback)
            {
                this.callback -= callback;
            }

            public void Invoke(object args)
            {
                if (this.actived is false)
                {
                    return;
                }

                this.value = args?.ToString();
                this.callback?.Invoke(this, args);
            }

            public void Dispose()
            {
                this.actived = false;
                this.name = String.Empty;
                this.path = String.Empty;
                this.window = null;
                this.gameObject = null;
                this.callback = null;
            }


            public void SetValue(string value)
            {
                SetTextWithoutNotify(value);
                Invoke(value);
            }

            public void SetTextWithoutNotify(string value)
            {
                if (gameObject == null)
                {
                    return;
                }

                this.value = value;
                TMP_InputField tmpInputField = gameObject.GetComponent<TMP_InputField>();
                if (tmpInputField == null)
                {
                    return;
                }

                tmpInputField.SetTextWithoutNotify(value);
            }
        }
    }
}