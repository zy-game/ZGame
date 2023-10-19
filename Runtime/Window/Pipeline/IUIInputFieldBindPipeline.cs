using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ZEngine.Window
{
    public interface IUIInputFieldBindPipeline : IUIComponentBindPipeline, IValueBindPipeline<string>
    {
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
            inputFieldBindPipelineHandle.inputField = tmpInputField;
            tmpInputField.onEndEdit.RemoveAllListeners();
            tmpInputField.onEndEdit.AddListener(args => inputFieldBindPipelineHandle.SetValue(args));
            inputFieldBindPipelineHandle.Active();
            return inputFieldBindPipelineHandle;
        }

        public static IUIInputFieldBindPipeline Create(UIWindow window, string path, string text, Action<string> callback)
        {
            IUIInputFieldBindPipeline inputFieldBindPipeline = Create(window, path);
            inputFieldBindPipeline.SetValueWithoutNotify(text);
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
            public TMP_InputField inputField { get; set; }
            private event Action<string> callback;

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

            public void AddListener(Action<string> callback)
            {
                this.callback += callback;
            }

            public void RemoveListener(Action<string> callback)
            {
                this.callback -= callback;
            }

            public void Invoke(string args)
            {
                if (this.actived is false)
                {
                    return;
                }

                this.callback?.Invoke(args);
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
                SetValueWithoutNotify(value);
                Invoke(value);
            }

            public void SetValueWithoutNotify(string value)
            {
                if (gameObject == null)
                {
                    return;
                }

                if (inputField == null)
                {
                    return;
                }

                this.value = value.ToString();
                inputField.SetTextWithoutNotify(this.value);
            }
        }
    }
}