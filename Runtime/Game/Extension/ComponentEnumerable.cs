using System;
using System.Collections;
using System.Collections.Generic;

namespace ZGame.Game
{
    public class ComponentEnumerable<T> : IReference, IEnumerable<T> where T : IComponent
    {
        private ComponentEnumerator<T> enumerator;

        public ComponentEnumerable(IComponent[] components)
        {
            if (components is null)
            {
                throw new ArgumentNullException(nameof(components));
            }

            enumerator = new ComponentEnumerator<T>(components);
        }

        public void Release()
        {
            // GameFrameworkFactory.Release(enumerator);
            // enumerator = null;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return enumerator;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public class ComponentEnumerator<T> : IReference, IEnumerator<T> where T : IComponent
    {
        private IComponent[] components;
        private int index = -1;

        public T Current
        {
            get
            {
                try
                {
                    return (T)components[index];
                }
                catch (IndexOutOfRangeException)
                {
                    throw new InvalidOperationException();
                }
            }
        }

        object IEnumerator.Current => Current;

        public ComponentEnumerator(IComponent[] components)
        {
            if (components is null)
            {
                throw new ArgumentNullException(nameof(components));
            }

            this.components = components;
        }

        public bool MoveNext()
        {
            if (components is null)
            {
                throw new InvalidOperationException();
            }

            index++;
            return (index < components.Length);
        }

        public void Reset()
        {
            index = 0;
        }


        public void Release()
        {
            // components = null;
        }
    }
}