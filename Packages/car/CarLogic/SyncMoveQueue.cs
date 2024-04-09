using System;
using System.Collections.Generic;
using UnityEngine;

namespace CarLogic
{
	public class SyncMoveQueue
	{
		private SyncTime syncTime;

		private readonly LinkedList<MovementData> pathQueue = new LinkedList<MovementData>();

		public float BeginTime;

		public float CurrTime;

		public bool HasNewData;

		private float queueLoseTime;

		private ushort lastIndex;

		private float startSyncTime;

		private float timeDelay;

		private bool _firstNotice;

		public LinkedListNode<MovementData> First => pathQueue.First;

		public LinkedListNode<MovementData> Last => pathQueue.Last;

		public MovementData FirstMovement => pathQueue.First.Value;

		public MovementData SecondMovement => pathQueue.First.Next.Value;

		public int Count => pathQueue.Count;

		public static event Action<string> OnMsg;

		public void MarkSyncTime(SyncTime syncTime)
		{
			this.syncTime = syncTime;
			if (startSyncTime <= 0f)
			{
				startSyncTime = syncTime.time;
			}
		}

		public void InitBeginTime(SyncTime syncTime, float delay)
		{
			this.syncTime = syncTime;
			if (CurrTime <= 0f && BeginTime <= 0f)
			{
				CurrTime = syncTime.time + delay;
				BeginTime = CurrTime;
				timeDelay = BeginTime - startSyncTime;
				if (timeDelay > 1f)
				{
					Debug.LogError($"时间轴延后稍大: {timeDelay}");
					float num = timeDelay - 1f;
					timeDelay -= num;
					CurrTime -= num;
					BeginTime -= num;
				}
				Debug.Log($"BeginTime >> {BeginTime}, DelayTime >> {timeDelay}");
			}
		}

		public void RepairTimeDelay()
		{
			if (syncTime != null)
			{
				CurrTime += timeDelay;
				BeginTime += timeDelay;
			}
		}

		public void AddQueueEnd(MovementData data)
		{
			if (data == null)
			{
				return;
			}
			lastIndex = data.Index;
			lastIndex = Math.Max(lastIndex, data.Index);
			if (pathQueue.Count > 0 && data.Index < pathQueue.Last.Value.Index)
			{
				if (data.Index < FirstMovement.Index || data.Index < SecondMovement.Index)
				{
					queueLoseTime += data.DeltaTime();
					return;
				}
				LinkedListNode<MovementData> linkedListNode = pathQueue.Last;
				while (data.Index < linkedListNode.Value.Index)
				{
					linkedListNode = linkedListNode.Previous;
				}
				if (data.Index == linkedListNode.Value.Index)
				{
					if (!_firstNotice)
					{
						Debug.LogError("[过滤]队列中存在相同Index");
						_firstNotice = true;
					}
				}
				else
				{
					pathQueue.AddAfter(linkedListNode, data);
				}
			}
			else if (pathQueue.Count > 0 && data.Index == pathQueue.Last.Value.Index)
			{
				if (!_firstNotice)
				{
					Debug.LogError("[过滤]队尾存在相同Index");
					_firstNotice = true;
				}
			}
			else
			{
				pathQueue.AddLast(data);
				HasNewData = true;
			}
		}

		public MovementData DelQueueHead()
		{
			LinkedListNode<MovementData> first = pathQueue.First;
			if (pathQueue.Count > 1)
			{
				pathQueue.RemoveFirst();
				FirstMovement.QueueLoseTime = queueLoseTime;
				queueLoseTime = 0f;
				CurrTime += FirstMovement.DeltaTime();
			}
			return first?.Value;
		}

		public List<MovementData> DelOverTime()
		{
			List<MovementData> list = new List<MovementData>();
			while (IsTimeOverSecond(syncTime.time))
			{
				MovementData movementData = DelQueueHead();
				if (movementData != null)
				{
					list.Add(movementData);
				}
			}
			return list;
		}

		private bool IsTimeOverSecond(float curTime)
		{
			if (HasNext())
			{
				return curTime > CurrTime + SecondMovement.DeltaTime();
			}
			return false;
		}

		public void DoCrashOut(HitWallDirection direction, Vector3 right, float start, float offset, float outTime, float holdTime, float backTime)
		{
			int num = 1;
			switch (direction)
			{
				default:
					return;
				case HitWallDirection.LEFT:
					num = -1;
					break;
				case HitWallDirection.RIGHT:
					num = 1;
					break;
			}
			float num2 = start + outTime + holdTime + backTime;
			float num3 = CurrTime;
			foreach (MovementData item in pathQueue)
			{
				num3 += item.DeltaTime();
				if (!item.Crashed && num3 >= start && num3 <= num2)
				{
					item.Crashed = true;
					if (num3 <= start + outTime)
					{
						float num4 = EaseLerp.EaseOutCirc(0f, 1f, (num3 - start) / outTime);
						item.Position += num * (right * offset * num4);
						item.EularAngle.y += (float)num * (15f - 15f * (1f - num4));
					}
					else if (num3 <= start + outTime + holdTime)
					{
						float num5 = EaseLerp.EaseOutBounce(1f, 0f, (num3 - start - outTime) / holdTime);
						item.Position += num * (right * offset);
						item.EularAngle.y += (float)num * (5f * num5);
					}
					else
					{
						float num6 = EaseLerp.Spring(1f, 0f, (num3 - start - outTime - holdTime) / backTime);
						item.Position += num * (right * offset * num6);
						item.EularAngle.y -= (float)num * (10f * num6);
					}
				}
			}
		}

		public bool HasNext()
		{
			return pathQueue.Count > 1;
		}

		public bool IsTimeBetweenFirstTwo(float curTime)
		{
			if (HasNext() && curTime >= CurrTime)
			{
				return curTime <= CurrTime + SecondMovement.DeltaTime();
			}
			return false;
		}

		public bool IsSimilarPosition()
		{
			if (pathQueue.Count > 1)
			{
				return (FirstMovement.Position - SecondMovement.Position).magnitude <= 0.1f;
			}
			return false;
		}

		public bool IsZeroVelocity()
		{
			if (FirstMovement.Speed <= 0f && (pathQueue.Count == 1 || SecondMovement.Velocity == Vector3.zero))
			{
				return true;
			}
			return false;
		}

		public void Clear()
		{
			pathQueue.Clear();
		}

		public void FixSpeed()
		{
			foreach (MovementData item in pathQueue)
			{
				if (item.Speed <= 0.02f)
				{
					item.Speed = 0f;
				}
			}
		}
	}
}
