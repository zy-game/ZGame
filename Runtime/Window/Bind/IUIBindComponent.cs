using System;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ZGame.Window
{
    public interface IUIBindComponent : IDisposable
    {
        string name { get; }
        string path { get; }
        GameObject gameObject { get; }
        void Setup(object args);
    }

    public interface IUIBindComponent<T> : IUIBindComponent
    {
        void Setup(T args);
    }

    public abstract class BasicBindComponent : IUIBindComponent
    {
        public string name { get; private set; }
        public string path { get; private set; }
        public GameObject gameObject { get; private set; }

        public BasicBindComponent(GameWindow window, string path)
        {
            this.path = path;
            name = Path.GetFileNameWithoutExtension(path);
            gameObject = window.gameObject.transform.Find(path)?.gameObject;
        }

        public abstract void Setup(object args);

        public virtual void Dispose()
        {
            this.gameObject = null;
            name = String.Empty;
            path = String.Empty;
        }
    }

    public sealed class SpriteComponent : BasicBindComponent, IUIBindComponent<Sprite>
    {
        private Image image;

        public SpriteComponent(GameWindow window, string path) : base(window, path)
        {
            if (gameObject == null)
            {
                return;
            }

            image = gameObject.GetComponent<Image>();
        }

        public override void Setup(object args)
        {
            this.Setup((Sprite)args);
        }

        public void Setup(Sprite args)
        {
            if (image == null)
            {
                return;
            }

            image.sprite = args;
        }
    }

    public sealed class TextureComponent : BasicBindComponent, IUIBindComponent<Texture>
    {
        private RawImage image;

        public TextureComponent(GameWindow window, string path) : base(window, path)
        {
            if (gameObject == null)
            {
                return;
            }

            image = gameObject.GetComponent<RawImage>();
        }

        public void Setup(Texture args)
        {
            if (image == null)
            {
                return;
            }

            image.texture = args;
        }

        public override void Setup(object args)
        {
            this.Setup((Texture)args);
        }
    }

    public sealed class LabelComponent : BasicBindComponent
    {
        private TMP_Text tmpText;

        public LabelComponent(GameWindow window, string path) : base(window, path)
        {
            if (gameObject == null)
            {
                return;
            }

            tmpText = gameObject.GetComponent<TMP_Text>();
        }

        public override void Setup(object args)
        {
            tmpText.text = args.ToString();
        }
    }

    public sealed class ToggleComponent : BasicBindComponent, IUIBindComponent<bool>
    {
        private Toggle toggle;

        public ToggleComponent(GameWindow window, string path) : base(window, path)
        {
            if (gameObject == null)
            {
                return;
            }

            toggle = gameObject.GetComponent<Toggle>();
        }

        public void Setup(bool args)
        {
            if (toggle == null)
            {
                return;
            }

            toggle.isOn = args;
        }

        public override void Setup(object args)
        {
            this.Setup((bool)args);
        }
    }


    public sealed class ButtonComponent : BasicBindComponent, IUIBindComponent<Action>
    {
        private Action onClick;
        private Button btn;

        public ButtonComponent(GameWindow window, string path) : base(window, path)
        {
            if (gameObject == null)
            {
                return;
            }

            btn = gameObject.GetComponent<Button>();
            if (btn == null)
            {
                return;
            }

            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(OnClickEvent);
        }

        private void OnClickEvent()
        {
            if (onClick is null)
            {
                return;
            }

            onClick?.Invoke();
        }

        public void Setup(Action args)
        {
            onClick = args;
        }


        public override void Setup(object args)
        {
            this.Setup((Action)args);
        }
    }

    public sealed class InputFieldComponent : BasicBindComponent, IUIBindComponent<Action<string>>
    {
        private TMP_InputField inputField;
        private Action<string> onCallback;

        public InputFieldComponent(GameWindow window, string path) : base(window, path)
        {
            if (gameObject == null)
            {
                return;
            }

            inputField = gameObject.GetComponent<TMP_InputField>();
            inputField.onEndEdit.RemoveAllListeners();
            inputField.onEndEdit.AddListener(OnEndEdit);
        }

        private void OnEndEdit(string args)
        {
            onCallback?.Invoke(args);
        }

        public void Setup(Action<string> args)
        {
            onCallback = args;
        }

        public override void Setup(object args)
        {
            this.Setup((Action<string>)args);
        }
    }
}