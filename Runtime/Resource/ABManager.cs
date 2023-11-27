using System.Collections.Generic;
using UnityEngine;

namespace ZGame.Resource
{
    class ABManager : Singleton<ABManager>
    {
        private List<ABHandle> _handles = new List<ABHandle>();

        internal ABHandle GetBundleHandle(ResHandle obj)
        {
            return _handles.Find(x => x.Contains(obj));
        }

        internal ABHandle GetBundleHandle(string path)
        {
            return _handles.Find(x => x.Contains(path));
        }

        public void Release(ResHandle handle)
        {
            ABHandle _handle = _handles.Find(x => x.Contains(handle));
            if (_handle is null)
            {
                return;
            }

            _handle.Release(handle);
        }

        internal void Remove(string name)
        {
            ABHandle handle = _handles.Find(x => x.name == name);
            if (handle is null)
            {
                return;
            }

            _handles.Remove(handle);
            handle.Dispose();
        }

        public ABHandle Add(string title)
        {
            var m = new ABHandle(title);
            _handles.Add(m);
            return m;
        }

        internal ABHandle Add(AssetBundle bundle)
        {
            ABHandle m = new ABHandle(bundle);
            _handles.Add(m);
            return m;
        }
    }
}