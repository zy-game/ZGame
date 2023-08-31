using System;
using System.Collections.Generic;
using UnityEngine;

namespace ZEngine
{
    internal class ClassManager : Single<ClassManager>
    {
        private Dictionary<Type, Stack<IReference>> _dic;

        public ClassManager()
        {
            _dic = new Dictionary<Type, Stack<IReference>>();
        }

        public IReference Dequeue(Type type)
        {
            if (!typeof(IReference).IsAssignableFrom(type))
            {
                throw new NotImplementedException(nameof(IReference));
            }

            if (Application.isPlaying is false)
            {
                return (IReference)Activator.CreateInstance(type);
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


        public T Dequeue<T>() where T : IReference
        {
            return (T)Dequeue(typeof(T));
        }

        public void Enqueue(IReference refresh)
        {
            if (refresh is null)
            {
                return;
            }

            if (Application.isPlaying is false)
            {
                return;
            }

            Type type = refresh.GetType();
            if (!_dic.TryGetValue(type, out Stack<IReference> stack))
            {
                _dic.Add(type, stack = new Stack<IReference>());
            }

            refresh.Release();
            if (stack.Count > ReferenceOptions.instance.MaxCount)
            {
                return;
            }

            stack.Push(refresh);
        }
    }
}