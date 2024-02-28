using System;
using System.Linq.Expressions;

namespace ZGame
{
    public sealed class Linked<T> : IDisposable
    {
        public class LinkNode<T> : IDisposable
        {
            public T value;
            public LinkNode<T> next;

            public LinkNode(T value)
            {
                this.value = value;
            }

            public void Dispose()
            {
                value = default;
                next = null;
                GC.SuppressFinalize(this);
            }
        }

        private LinkNode<T> _head;

        /// <summary>
        /// 将数据添加到链表尾部
        /// </summary>
        /// <param name="value"></param>
        public void Add(T value)
        {
            LinkNode<T> newNode = new LinkNode<T>(value);

            if (_head == null)
            {
                _head = newNode;
                return;
            }

            LinkNode<T> lastNode = _head;
            while (lastNode.next != null)
            {
                lastNode = lastNode.next;
            }

            lastNode.next = newNode;
        }

        public void Remove(T value)
        {
            if (_head is null)
            {
                return;
            }

            LinkNode<T> temp = _head;

            // 如果要删除的节点是头节点
            if (temp != null && temp.value.Equals(value))
            {
                _head = temp.next;
                temp.Dispose();
                return;
            }

            LinkNode<T> prev = null;
            // 查找要删除的节点，同时记录前一个节点
            while (temp != null && temp.value.Equals(value) is false)
            {
                prev = temp;
                temp = temp.next;
            }

            // 如果找到了要删除的节点
            if (temp == null)
            {
                return;
            }

            // 从链表中移除节点
            prev.next = temp.next;
            temp.Dispose();
        }

        /// <summary>
        /// 清理所有的数据
        /// </summary>
        public void Clear()
        {
            LinkNode<T> temp = _head;
            while (temp != null)
            {
                temp = temp.next;
                temp.Dispose();
            }

            _head = null;
        }

        /// <summary>
        /// 是否存在指定的数据
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool Contains(T value)
        {
            return Find(value) is not null;
        }

        /// <summary>
        /// 在指定的数据之后添加数据
        /// </summary>
        /// <param name="value"></param>
        /// <param name="after"></param>
        public void AddAfter(T value, T after)
        {
            LinkNode<T> afterNode = Find(after);
            if (afterNode is null)
            {
                Add(value);
                return;
            }

            LinkNode<T> newNode = new LinkNode<T>(value);
            LinkNode<T> nextNode = afterNode.next;
            afterNode.next = newNode;
            newNode.next = nextNode;
        }

        /// <summary>
        /// 在指定的数据之前添加数据
        /// </summary>
        /// <param name="value"></param>
        /// <param name="before"></param>
        public void AddBefore(T value, T before)
        {
            LinkNode<T> beforeNode = Find(before);
            if (beforeNode is null)
            {
                Add(value);
                return;
            }

            LinkNode<T> newNode = new LinkNode<T>(value);
            beforeNode.next = newNode;
            newNode.next = beforeNode.next;
        }

        /// <summary>
        /// 查找指定的数据
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public LinkNode<T> Find(T value)
        {
            LinkNode<T> temp = _head;
            while (temp is not null)
            {
                if (temp.value.Equals(value))
                {
                    break;
                }

                temp = temp.next;
            }

            return temp;
        }

        /// <summary>
        /// 查找指定的数据
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public LinkNode<T> Find(Func<T, bool> predicate)
        {
            LinkNode<T> temp = _head;
            while (temp is not null)
            {
                if (predicate(temp.value))
                {
                    break;
                }

                temp = temp.next;
            }

            return temp;
        }

        public void Dispose()
        {
            Clear();
            GC.SuppressFinalize(this);
        }
    }
}