using System;
using UnityEngine;

namespace CarLogic
{
	public class CarSkillLongerSmallGasForChipFastSpeed : CarChipTechnologySkillBase
	{
		private bool _hadEffect;

		private Float _probability = 0;

		private Float _exTime = 0;

		private Float _duration = 0;

		private Float countDownTime = 0;

		public override CarSkillId ID => CarSkillId.FastSpeed;

		public override void Init(int param1, int param2, int param3)
		{
			_exTime = (float)param1 * 0.01f * 0.01f;
			_duration = param3;
			_hadEffect = false;
		}

		public override void OnActiveChange(bool active)
		{
			if (carState != null)
			{
				CarSkillDipatcher instance = Singleton<CarSkillDipatcher>.Instance;
				instance.OnFastSpeedComplete = (Action)Delegate.Remove(instance.OnFastSpeedComplete, new Action(OnFastSpeedComplete));
				CarView view = carState.view;
				view.OnUpdate = (Action)Delegate.Remove(view.OnUpdate, new Action(OnUpdate));
				CarCallBack callBacks = carState.CallBacks;
				callBacks.OnAffectedByItem = (Action<RaceItemId, CarState, ItemCallbackType, object>)Delegate.Remove(callBacks.OnAffectedByItem, new Action<RaceItemId, CarState, ItemCallbackType, object>(onAffectByItem));
				if (active)
				{
					CarSkillDipatcher instance2 = Singleton<CarSkillDipatcher>.Instance;
					instance2.OnFastSpeedComplete = (Action)Delegate.Combine(instance2.OnFastSpeedComplete, new Action(OnFastSpeedComplete));
					CarView view2 = carState.view;
					view2.OnUpdate = (Action)Delegate.Combine(view2.OnUpdate, new Action(OnUpdate));
					CarCallBack callBacks2 = carState.CallBacks;
					callBacks2.OnAffectedByItem = (Action<RaceItemId, CarState, ItemCallbackType, object>)Delegate.Combine(callBacks2.OnAffectedByItem, new Action<RaceItemId, CarState, ItemCallbackType, object>(onAffectByItem));
				}
			}
		}

		public override bool Usable()
		{
			return true;
		}

		private void OnUpdate()
		{
			if ((float)countDownTime > 0f)
			{
				countDownTime = (float)countDownTime - Time.deltaTime;
			}
			else
			{
				countDownTime = 0;
			}
		}

		private void OnFastSpeedComplete()
		{
			if ((float)countDownTime <= 0f && Usable() && SkillUsable())
			{
				ChipTechnologyToggle();
				countDownTime = _duration;
			}
		}

		public override void Toggle()
		{
			base.Toggle();
		}

		private void onAffectByItem(RaceItemId id, CarState state, ItemCallbackType type, object obj)
		{
			if (!(obj is CommonGasItem commonGasItem))
			{
				return;
			}
			switch (type)
			{
				case ItemCallbackType.AFFECT:
					if (commonGasItem.Level == 2 && (float)countDownTime > 0f && !_hadEffect)
					{
						commonGasItem.duration += commonGasItemDuration * (float)_exTime;
						Toggle();
						_hadEffect = true;
					}
					break;
				case ItemCallbackType.BREAK:
					if (commonGasItem.Level == 2 && _hadEffect)
					{
						Stop();
					}
					break;
			}
		}

		public override void Break()
		{
			Stop();
			base.Break();
		}
	}
}
