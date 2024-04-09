using System;
using System.Collections.Generic;
using UnityEngine;

namespace CarLogic
{
	[Serializable]
	public class PathAnimationController : ControllerBase
	{
		private List<Collider> delayAnims = new List<Collider>();

		private CarState carState;

		public override void OnActiveChange(bool active)
		{
			CarView view = carState.view;
			view.OnTriggerBegin = (Action<Collider>)Delegate.Remove(view.OnTriggerBegin, new Action<Collider>(onTriggerSpecial));
			CarCallBack callBacks = carState.CallBacks;
			callBacks.OnReachEnd = (Action<bool>)Delegate.Remove(callBacks.OnReachEnd, new Action<bool>(OnReachEnd));
			if (active)
			{
				CarView view2 = carState.view;
				view2.OnTriggerBegin = (Action<Collider>)Delegate.Combine(view2.OnTriggerBegin, new Action<Collider>(onTriggerSpecial));
				CarCallBack callBacks2 = carState.CallBacks;
				callBacks2.OnReachEnd = (Action<bool>)Delegate.Combine(callBacks2.OnReachEnd, new Action<bool>(OnReachEnd));
			}
		}

		public void Init(CarState carState)
		{
			this.carState = carState;
		}

		private void OnReachEnd(bool end)
		{
			if (!end)
			{
				return;
			}
			for (int i = 0; i < delayAnims.Count; i++)
			{
				Collider collider = delayAnims[i];
				AnimationTriggerData component = collider.GetComponent<AnimationTriggerData>();
				if (component.State != 0)
				{
					AnimationTrigger animationTrigger = new AnimationTrigger(collider, component);
					animationTrigger.Toggle(carState);
				}
			}
		}

		private void onTriggerSpecial(Collider c)
		{
			int layer = c.gameObject.layer;
			if (layer == 23)
			{
				onToggleAnimationTrigger(c);
			}
		}

		private void onToggleAnimationTrigger(Collider c)
		{
			AnimationTriggerData component = c.GetComponent<AnimationTriggerData>();
			if (component == null)
			{
				Debug.LogWarning("Trigger Animation lose AnimationData");
				return;
			}
			if (component.State == AnimationTriggerData.TriggerState.RUNNING)
			{
				Debug.Log("Trigger Animation is Running,name:" + c.name);
				return;
			}
			switch (component.Target)
			{
				case AnimationTriggerData.TriggerTarget.ONLY_SELF:
					if (carState.CarPlayerType != 0)
					{
						return;
					}
					break;
				case AnimationTriggerData.TriggerTarget.ORTHERS:
					if (carState.CarPlayerType == PlayerType.PLAYER_SELF)
					{
						return;
					}
					break;
			}
			switch (component.Type)
			{
				case AnimationTriggerData.TriggerType.RACE_END:
					if (carState.ReachedEnd)
					{
						AnimationTrigger animationTrigger2 = new AnimationTrigger(c, component);
						animationTrigger2.Toggle(carState);
					}
					else if (delayAnims != null && !delayAnims.Contains(c))
					{
						delayAnims.Add(c);
					}
					break;
				case AnimationTriggerData.TriggerType.EVERY_TIMES:
				{
					AnimationTrigger animationTrigger = new AnimationTrigger(c, component);
					animationTrigger.Toggle(carState);
					break;
				}
			}
		}
	}
}
