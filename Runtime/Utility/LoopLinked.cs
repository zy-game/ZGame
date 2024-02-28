using System;

namespace ZGame
{
    public sealed class LoopLinked<T> : IDisposable
    {
        public class LinkNode<T> : IDisposable
        {
            public T value;
            public LinkNode<T> next;
            public LinkNode<T> prev;

            public LinkNode(T value)
            {
                this.value = value;
            }

            public void Dispose()
            {
                value = default;
                next = default;
                prev = default;
                GC.SuppressFinalize(this);
            }
        }

        private LinkNode<T> _head;
        private LinkNode<T> _tall;

        public LoopLinked()
        {
            _head = new LinkNode<T>(default);
            _tall = new LinkNode<T>(default);
            _head.next = _tall;
            _head.prev = _tall;
            _tall.next = _head;
            _tall.prev = _head;
        }

        public void Add(T value)
        {
            LinkNode<T> node = new LinkNode<T>(value);
            _tall.prev.next = node;
            node.prev = _tall.prev;
            _tall.prev = node;
            node.next = _tall;
        }

        public void Remove(T value)
        {
            LinkNode<T> node = Find(value);
            if (node is null)
            {
                return;
            }

            node.prev.next = node.next;
            node.next.prev = node.prev;
            node.Dispose();
        }

        public void Clear()
        {
            LinkNode<T> temp = _head.next;
            while (temp is not null && temp.Equals(_tall) is false)
            {
                temp.Dispose();
                temp = temp.next;
            }

            _head.next = _tall;
            _head.prev = _tall;
            _tall.next = _head;
            _tall.prev = _head;
        }

        public bool Contains(T value)
        {
            return Find(value) is not null;
        }

        public void AddAfter(T value, T after)
        {
            LinkNode<T> afterNode = Find(after);
            if (afterNode is null)
            {
                return;
            }

            LinkNode<T> node = new LinkNode<T>(value);
            afterNode.next.prev = node;
            node.next = afterNode.next;
            afterNode.next = node;
            node.prev = afterNode;
        }

        public void AddBefore(T value, T before)
        {
            LinkNode<T> beforeNode = Find(before);
            if (beforeNode is null)
            {
                return;
            }

            LinkNode<T> node = new LinkNode<T>(value);
            beforeNode.prev.next = node;
            node.prev = beforeNode.prev;
            beforeNode.prev = node;
            node.next = beforeNode;
        }

        public LinkNode<T> Find(T value)
        {
            LinkNode<T> temp = _head.next;
            while (temp is not null && temp.Equals(_tall) is false && temp.value.Equals(value) is false)
            {
                temp = temp.next;
            }

            return temp;
        }

        public LinkNode<T> Find(Func<T, bool> predicate)
        {
            LinkNode<T> temp = _head.next;
            while (temp is not null && temp.Equals(_tall) is false && predicate(temp.value) is false)
            {
                temp = temp.next;
            }

            return temp;
        }

        public void Dispose()
        {
            Clear();
            _head = null;
            _tall = null;
            GC.SuppressFinalize(this);
        }
    }
}