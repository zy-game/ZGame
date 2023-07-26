using System;
using System.Collections.Generic;

namespace ZEngine.Core
{
    internal class ReferenceManager : Single<ReferenceManager>
    {
        private Dictionary<Type, Stack<IReference>> _dic;

        public ReferenceManager()
        {
            _dic = new Dictionary<Type, Stack<IReference>>();
        }

        public IReference Dequeue(Type type)
        {
            if (!typeof(IReference).IsAssignableFrom(type))
            {
                throw new NotImplementedException(nameof(IReference));
            }

            if (!_dic.TryGetValue(type, out Stack<IReference> stack))
            {
                _dic.Add(type, stack = new Stack<IReference>());
            }

            if (!stack.TryPop(out IReference result))
            {
                result = (IReference)Activator.CreateInstance(type);
            }

            return result;
        }


        public T Dequeue<T>()
        {
            return (T)Dequeue(typeof(T));
        }

        public void Enqueue(IReference refresh)
        {
            Type type = refresh.GetType();
            if (!_dic.TryGetValue(type, out Stack<IReference> stack))
            {
                _dic.Add(type, stack = new Stack<IReference>());
            }

            refresh.Release();
            if (stack.Count > AppConfig.instance.refreshOptions.MaxCount)
            {
                return;
            }

            stack.Push(refresh);
        }
    }
}