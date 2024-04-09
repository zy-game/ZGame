using System;
using UnityEngine;

namespace CarLogic
{
	public abstract class PassiveTrigger
	{
		public Action<PassiveTrigger> OnOver = delegate
		{
		};

		public object UserData;

		protected ushort instanceId;

		public long User;

		protected bool manualPause;

		protected RaceItemStatus status;

		protected CarState carState;

		public abstract RaceItemId ItemId { get; }

		public ushort InstanceId => instanceId;

		public bool ManualPaused => manualPause;

		public GameObject ItemObject { get; set; }

		public RaceItemStatus ItemStatus => status;

		public PassiveTrigger(ushort id, long playerWorldId = 0L)
		{
			instanceId = id;
			User = playerWorldId;
		}

		public virtual void ApplyEffect(ItemParams ps)
		{
			carState = ps.user;
			if (carState != null && carState.CallBacks.OnAffectedByItem != null)
			{
				carState.CallBacks.OnAffectedByItem(ItemId, carState, ItemCallbackType.TOGGLE, this);
			}
			status = RaceItemStatus.APPLYING;
		}

		public virtual void Release()
		{
			if ((bool)ItemObject)
			{
				UnityEngine.Object.Destroy(ItemObject);
			}
		}

		public virtual void Stop()
		{
			if (OnOver != null)
			{
				OnOver(this);
			}
			if (carState != null && carState.CallBacks.OnAffectedByItem != null)
			{
				carState.CallBacks.OnAffectedByItem(ItemId, carState, ItemCallbackType.BREAK, this);
			}
			status = RaceItemStatus.NONE;
		}
	}
}
