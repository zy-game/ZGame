using System;
using System.Collections.Generic;

namespace ZGame.Data
{
    class ObjectPooolHandle : IDisposable
    {
        private List<object> objList;

        public ObjectPooolHandle()
        {
            objList = new List<object>();
        }

        public void Popup()
        {
        }

        public void Popup(object key)
        {
        }

        public void Dispose()
        {
        }
    }

    public class ObjectPool : GameFrameworkModule
    {
        private Dictionary<Type, ObjectPooolHandle> _handles;

        public override void OnAwake()
        {
            _handles = new Dictionary<Type, ObjectPooolHandle>();
        }

        public T Popup<T>(object key)
        {
            return default;
        }

        public void Push(object key, object value)
        {
        }

        public int Count<T>()
        {
            return 0;
        }

        public List<T> All<T>()
        {
            return default;
        }
    }
}