using System.Collections;
using System.Collections.Generic;

namespace ZGame.Game
{
    public class ComponentEnumerable<T> : IReferenceObject, IEnumerable<T> where T : IComponent
    {
        private ComponentEnumerator<T> enumerator;

        public static ComponentEnumerable<T> Create(IComponent[] components)
        {
            ComponentEnumerable<T> enumerable = GameFrameworkFactory.Spawner<ComponentEnumerable<T>>();
            enumerable.enumerator = ComponentEnumerator<T>.Create(components);
            return enumerable;
        }

        public void Release()
        {
            GameFrameworkFactory.Release(enumerator);
            enumerator = null;
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

    public class ComponentEnumerator<T> : IReferenceObject, IEnumerator<T> where T : IComponent
    {
        private IComponent[] components;
        private int index = 0;
        private T current;
        public T Current => current;

        object IEnumerator.Current => current;

        public static ComponentEnumerator<T> Create(IComponent[] components)
        {
            ComponentEnumerator<T> queryable = GameFrameworkFactory.Spawner<ComponentEnumerator<T>>();
            queryable.components = components;
            return queryable;
        }

        public bool MoveNext()
        {
            if (components is null || components.Length == 0)
            {
                return false;
            }

            current = (T)components[index];
            index++;
            if (index > components.Length - 1)
            {
                return false;
            }

            return true;
        }

        public void Reset()
        {
            index = 0;
        }


        public void Release()
        {
            components = null;
        }
    }
}