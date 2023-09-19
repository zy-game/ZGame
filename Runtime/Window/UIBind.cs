using UnityEngine;

namespace ZEngine.Window
{
    public interface IUIBind<T>
    {
        public static IUIBind<T> Create(GameObject gameObject)
        {
            return default;
        }

        void Change(T value);

        T GetValue();
    }

    public class BindObject : MonoBehaviour, IUIBind<object>
    {
        public void Change(object value)
        {
            throw new System.NotImplementedException();
        }

        public object GetValue()
        {
            throw new System.NotImplementedException();
        }
    }

    public class BindSprite : MonoBehaviour, IUIBind<Sprite>
    {
        public void Change(Sprite value)
        {
            throw new System.NotImplementedException();
        }

        public Sprite GetValue()
        {
            throw new System.NotImplementedException();
        }
    }
}