using System;
using UnityEngine;

namespace CarLogic
{
	public abstract class RaceItemBase
	{
		public static AnimationCurve floatCurve;

		private static int sid;

		private int iid = sid++;

		public Action<GameObject, TriggerData> LoadFinishCallback;

		protected CarState carState;

		protected CarModel carModel;

		protected ItemParams itemParams;

		protected float exhanceTime;

		protected bool manualPause;

		protected CarState[] targets;

		public object UserData;

		protected RaceItemStatus status;

		protected Action<RaceItemBase, ItemCallbackType> CallBack = delegate
		{
		};

		public abstract RaceItemId ItemId { get; }

		public int IID => iid;

		public bool ManualPaused => manualPause;

		public CarState[] Targets
		{
			get
			{
				if (itemParams == null)
				{
					Debug.LogWarning("Null itemParams " + ItemId);
					return null;
				}
				return itemParams.targets;
			}
		}

		public CarState User
		{
			get
			{
				if (itemParams == null)
				{
					Debug.LogWarning("Null itemParams " + ItemId);
					return null;
				}
				return itemParams.user;
			}
		}

		public ushort InstanceId
		{
			get
			{
				if (itemParams != null)
				{
					return itemParams.instanceId;
				}
				return 0;
			}
		}

		public ItemParams Params => itemParams;

		public RaceItemStatus ItemStatus => status;

		static RaceItemBase()
		{
			floatCurve = new AnimationCurve();
			sid = 0;
			floatCurve.postWrapMode = WrapMode.PingPong;
			floatCurve.AddKey(0f, 0.12f);
			floatCurve.AddKey(0.1f, 1f);
			floatCurve.AddKey(0.4f, 0.9f);
			floatCurve.AddKey(0.5f, 1f);
			floatCurve.AddKey(0.6f, 0.9f);
			floatCurve.AddKey(0.7f, 1f);
			floatCurve.AddKey(0.8f, 0.9f);
			floatCurve.AddKey(0.9f, 1f);
			floatCurve.AddKey(1f, 0.9f);
		}

		public RaceItemBase()
		{
			init();
		}

		public RaceItemBase(CarState state)
		{
			carState = state;
			init();
		}

		public RaceItemBase(CarState user, CarState[] targets)
		{
			if (targets != null)
			{
				this.targets = targets;
			}
			carState = user;
			init();
		}

		protected virtual void init()
		{
			if (itemParams == null)
			{
				itemParams = new ItemParams(null, null, 0);
			}
		}

		~RaceItemBase()
		{
		}

		public virtual void OnReceive(byte[] data)
		{
		}

		public virtual bool Usable(ItemParams ps)
		{
			return true;
		}

		public virtual void Toggle(ItemParams ps)
		{
			itemParams = ps;
			status = RaceItemStatus.APPLYING;
			this.carState = ps.user;
			targets = ps.targets;
			if (targets != null)
			{
				for (int i = 0; i < targets.Length; i++)
				{
					CarState carState = targets[i];
					if (carState.CallBacks.OnAffectedByItem != null)
					{
						carState.CallBacks.OnAffectedByItem(ItemId, carState, ItemCallbackType.TOGGLE, this);
					}
				}
			}
			CallBack(this, ItemCallbackType.TOGGLE);
		}

		public virtual void ToggleEffect(ItemParams ps)
		{
		}

		public virtual void ApplyData(ItemParams ps)
		{
			if (targets != null)
			{
				targets = ps.targets;
			}
		}

		public virtual void OnNullTarget()
		{
		}

		public virtual void Enhance(float addition)
		{
			exhanceTime += addition;
		}

		public virtual void Break()
		{
			CallBack(this, ItemCallbackType.BREAK);
			status = RaceItemStatus.NONE;
			if (targets == null)
			{
				return;
			}
			for (int i = 0; i < targets.Length; i++)
			{
				CarState carState = targets[i];
				if (carState != null && carState.CallBacks.OnAffectedByItem != null)
				{
					carState.CallBacks.OnAffectedByItem(ItemId, carState, ItemCallbackType.BREAK, this);
				}
			}
		}

		protected virtual void saveToBuffer(TriggerData data)
		{
			RaceCallback.SaveTriggerToBuf(data);
		}

		protected virtual void addReleaseCallback(AbstractView view, TriggerData data)
		{
			RaceCallback.ReleaseTrigger(view, data);
		}

		public virtual void SendRequest(ItemParams ps)
		{
			status = RaceItemStatus.COMMIT_REQUEST;
		}

		public virtual void AddCallback(Action<RaceItemBase, ItemCallbackType> callback)
		{
			CallBack = (Action<RaceItemBase, ItemCallbackType>)Delegate.Combine(CallBack, callback);
		}

		public virtual void RemoveCallback(Action<RaceItemBase, ItemCallbackType> callback)
		{
			CallBack = (Action<RaceItemBase, ItemCallbackType>)Delegate.Remove(CallBack, callback);
		}
	}
}
