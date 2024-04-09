using System;
using System.Collections.Generic;

namespace CarLogic
{
	public class SyncMoveQueueForecast
	{
		private SyncTimeForecast syncTime;

		private readonly LinkedList<MovementDataForecast> pathQueue = new LinkedList<MovementDataForecast>();

		public long startSyncTime;

		public bool HasNewData;

		private ushort lastIndex;

		private Action<int> onChangeMove;

		public LinkedListNode<MovementDataForecast> First => pathQueue.First;

		public LinkedListNode<MovementDataForecast> Last => pathQueue.Last;

		public MovementDataForecast FirstMovement => pathQueue.First.Value;

		public MovementDataForecast SecondMovement => pathQueue.First.Next.Value;

		public int Count => pathQueue.Count;

		public void initQueue(SyncTimeForecast syncTime, Action<int> onChangeMove)
		{
			onChangeMove = onChangeMove;
			this.syncTime = syncTime;
			if (startSyncTime <= 0)
			{
				startSyncTime = syncTime.time;
			}
		}

		public void AddQueueEnd(MovementDataForecast data)
		{
			if (data == null)
			{
				return;
			}
			lastIndex = data.Index;
			lastIndex = Math.Max(lastIndex, data.Index);
			if (pathQueue.Count > 0 && data.Index < pathQueue.Last.Value.Index)
			{
				if (data.Index >= FirstMovement.Index && data.Index >= SecondMovement.Index)
				{
					LinkedListNode<MovementDataForecast> linkedListNode = pathQueue.First;
					while (data.Index > linkedListNode.Value.Index)
					{
						linkedListNode = linkedListNode.Next;
					}
					pathQueue.AddBefore(linkedListNode, data);
				}
			}
			else
			{
				pathQueue.AddLast(data);
				HasNewData = true;
			}
		}

		public MovementDataForecast DelQueueHead()
		{
			LinkedListNode<MovementDataForecast> first = pathQueue.First;
			if (pathQueue.Count > 1)
			{
				pathQueue.RemoveFirst();
				if (onChangeMove != null)
				{
					onChangeMove(0);
				}
			}
			return first?.Value;
		}

		public List<MovementDataForecast> DelOverTime()
		{
			List<MovementDataForecast> list = new List<MovementDataForecast>();
			while (IsTimeOverSecond())
			{
				MovementDataForecast movementDataForecast = DelQueueHead();
				if (movementDataForecast != null)
				{
					list.Add(movementDataForecast);
				}
				if (pathQueue.Count <= 1)
				{
					break;
				}
			}
			return list;
		}

		private bool IsTimeOverSecond()
		{
			if (pathQueue.Count >= 1)
			{
				return syncTime.time - FirstMovement.m_momentTime > 10;
			}
			return false;
		}

		public bool HasNext()
		{
			return pathQueue.Count > 1;
		}

		public void Clear()
		{
			pathQueue.Clear();
		}
	}
}
