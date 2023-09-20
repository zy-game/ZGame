using UnityEngine;

namespace ZEngine.Window
{
    public interface IUIBind<T> : IReference
    {
        T value { get; }
        GameObject gameObject { get; }
        void Change(T value);
    }

    public abstract class BindBasic<T> : IUIBind<T>
    {
        private T _value;
        public GameObject gameObject { get; protected set; }

        public T value
        {
            get => _value;
        }

        public void Change(T value)
        {
            _value = value;
            OnChange();
        }

        public virtual void Release()
        {
        }

        protected abstract void OnChange();
    }

    public class BindObject : BindBasic<object>
    {
        protected override void OnChange()
        {
        }
    }

    public class BindSprite : BindBasic<Sprite>
    {
        protected override void OnChange()
        {
        }
    }

    public class BindTexture : BindBasic<Texture2D>
    {
        protected override void OnChange()
        {
        }
    }

    public class BindInputField : BindBasic<object>
    {
        protected override void OnChange()
        {
        }
    }

    public class BindButton : BindObject
    {
    }

    public class BindToggle : BindBasic<bool>
    {
        protected override void OnChange()
        {
        }
    }

    public class BindSlider : BindBasic<float>
    {
        protected override void OnChange()
        {
        }
    }

    public class BindScrollbar : BindBasic<float>
    {
        protected override void OnChange()
        {
        }
    }

    public class BindScrollView : BindBasic<float>
    {
        protected override void OnChange()
        {
        }
    }
}