namespace UdpAdapterClient
{
	public class safeNoLockQueue<T>
	{
		private class Node<K>
		{
			internal K Item;

			internal Node<K> Next;

			public Node(K item, Node<K> next)
			{
				Item = item;
				Next = next;
			}
		}

		private Node<T> _head;

		private Node<T> _tail;

		public bool IsEmpty => _head.Next == null;

		public safeNoLockQueue()
		{
			_head = new Node<T>(default(T), null);
			_tail = _head;
		}

		public void Enqueue(T item)
		{
			Node<T> node = new Node<T>(item, null);
			_tail.Next = node;
			_tail = node;
		}

		public T Dequeue()
		{
			Node<T> next = _head.Next;
			if (next == null)
			{
				return default(T);
			}
			_head.Next = null;
			_head = next;
			T item = next.Item;
			next.Item = default(T);
			return item;
		}

		public void Clear()
		{
			do
			{
				Dequeue();
			}
			while (!IsEmpty);
		}
	}
}
