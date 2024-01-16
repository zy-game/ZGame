using System;

namespace ZGame
{
    public class LoopLinked<T>
    {
        public class LinkedNode : IDisposable
        {
            public T current { get; private set; }
            public LinkedNode next { get; internal set; }
            public LinkedNode prev { get; internal set; }

            internal LinkedNode(T data)
            {
                current = data;
            }

            public void Dispose()
            {
                if (next != null)
                {
                    next.prev = prev;
                }

                if (prev != null)
                {
                    prev.next = next;
                }

                next = null;
                prev = null;
                current = default;
            }
        }

        public LinkedNode head;
        public LinkedNode tail;

        public T current
        {
            get
            {
                if (curNode is null)
                {
                    return default;
                }

                return curNode.current;
            }
        }

        public LinkedNode curNode { get; private set; }

        public LinkedNode GetNode(T data)
        {
            if (head == null)
            {
                return null;
            }

            for (LinkedNode temp = head; temp != null; temp = temp.next)
            {
                if (temp.current.Equals(data))
                {
                    return temp;
                }
            }

            return null;
        }

        public void Switch(T data)
        {
            LinkedNode temp = GetNode(data);
            if (temp is null || curNode == temp)
            {
                return;
            }

            curNode = temp;
        }

        public void SwitchNext()
        {
            if (curNode == null)
            {
                return;
            }

            curNode = curNode.next;
        }

        public void SwitchPrev()
        {
            if (curNode == null)
            {
                return;
            }

            curNode = curNode.prev;
        }

        public void AddLast(T data)
        {
            LinkedNode node = new LinkedNode(data);
            if (tail == null)
            {
                head = node;
                tail = head;
                head.next = tail;
                head.prev = tail;
                tail.next = head;
                tail.prev = head;
                return;
            }

            node.next = head;
            node.prev = tail;

            tail.next = node;
            head.prev = node;
            tail = node;
        }

        public void AddFirst(T data)
        {
            LinkedNode node = new LinkedNode(data);
            if (head == null)
            {
                head = node;
                tail = head;
                head.next = tail;
                head.prev = tail;
                tail.next = head;
                tail.prev = head;
                return;
            }

            node.next = head;
            node.prev = tail;
            head.prev = node;
            tail.next = node;
            head = node;
        }

        public void Remove(T data)
        {
            LinkedNode temp = GetNode(data);
            if (temp is null)
            {
                return;
            }

            temp.Dispose();
        }
    }
}