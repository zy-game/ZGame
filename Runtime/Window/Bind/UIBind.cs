using System;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ZGame.Window
{
    public class UIBind<T> : IDisposable where T : Component
    {
        public string name { get; private set; }
        public Transform transform { get; private set; }
        public GameObject gameObject { get; private set; }
        public T Component { get; private set; }

        public UIBind(Transform transform)
        {
            if (transform == null)
            {
                return;
            }

            name = transform.name;
            this.transform = transform;
            gameObject = transform.gameObject;
            this.Component = gameObject.GetComponent<T>();
        }

        public void Setup(object args)
        {
            switch (Component)
            {
                case RectTransform rectTransform:
                    break;
                case Image image:
                    image.sprite = (Sprite)args;
                    break;
                case RawImage rawImage:
                    rawImage.texture = (Texture)args;
                    break;
                case TextMeshProUGUI text:
                    text.text = (string)args;
                    break;
                case Slider slider:
                    if (args is UnityAction<float> callback)
                    {
                        slider.onValueChanged.AddListener(callback);
                    }
                    else
                    {
                        slider.value = (float)args;
                    }

                    break;
                case TMP_InputField inputField:
                    if (args is UnityAction<string> endedit)
                    {
                        inputField.onEndEdit.AddListener(endedit);
                    }
                    else
                    {
                        inputField.text = (string)args;
                    }

                    break;
                case Toggle toggle:
                    if (args is UnityAction<bool> change)
                    {
                        toggle.onValueChanged.AddListener(change);
                    }
                    else
                    {
                        toggle.isOn = (bool)args;
                    }

                    break;
                case Button button:
                    if (args is UnityAction action)
                    {
                        button.onClick.AddListener(action);
                    }

                    break;
            }
        }

        public virtual void Dispose()
        {
            this.transform = null;
            name = String.Empty;
        }
    }
}