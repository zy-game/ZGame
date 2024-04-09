using System;
using UnityEngine;

namespace CarLogic
{
	public class CarSkillFasterBigGasForChipSpeedUp : CarChipTechnologySkillBase
	{
		public Float Duration;

		private Int _exForce = 0;

		private Float countDownTime = 0;

		public Float Probability { get; set; }

		public override CarSkillId ID => CarSkillId.FasterBigGasSpeedUp;

		public override void OnActiveChange(bool active)
		{
			if (carState != null)
			{
				CarCallBack callBacks = carState.CallBacks;
				callBacks.OnSpecialCallback = (Action<CarState, SpecialTriggerBase, SpecialCallback>)Delegate.Remove(callBacks.OnSpecialCallback, new Action<CarState, SpecialTriggerBase, SpecialCallback>(onSpecialCallback));
				CarCallBack callBacks2 = carState.CallBacks;
				callBacks2.OnAffectedByItem = (Action<RaceItemId, CarState, ItemCallbackType, object>)Delegate.Remove(callBacks2.OnAffectedByItem, new Action<RaceItemId, CarState, ItemCallbackType, object>(onAffectByItem));
				CarView view = carState.view;
				view.OnUpdate = (Action)Delegate.Remove(view.OnUpdate, new Action(OnUpdate));
				if (active)
				{
					CarCallBack callBacks3 = carState.CallBacks;
					callBacks3.OnSpecialCallback = (Action<CarState, SpecialTriggerBase, SpecialCallback>)Delegate.Combine(callBacks3.OnSpecialCallback, new Action<CarState, SpecialTriggerBase, SpecialCallback>(onSpecialCallback));
					CarCallBack callBacks4 = carState.CallBacks;
					callBacks4.OnAffectedByItem = (Action<RaceItemId, CarState, ItemCallbackType, object>)Delegate.Combine(callBacks4.OnAffectedByItem, new Action<RaceItemId, CarState, ItemCallbackType, object>(onAffectByItem));
					CarView view2 = carState.view;
					view2.OnUpdate = (Action)Delegate.Combine(view2.OnUpdate, new Action(OnUpdate));
				}
			}
		}

		public override void Init(int param1, int param2, int param3)
		{
			_exForce = (float)param1 * 0.01f;
			Probability = param2;
			Duration = param3;
			HadEffect = false;
		}

		public override bool Usable()
		{
			return (float)getRandomNum() < (float)Probability;
		}

		protected void onSpecialCallback(CarState car, SpecialTriggerBase sp, SpecialCallback type)
		{
			if (car.view.CarPlayerType != 0)
			{
				return;
			}
			if (sp.Type == SpecialType.SpeedUp && type == SpecialCallback.Toggle)
			{
				if ((float)countDownTime <= 0f && Usable() && SkillUsable())
				{
					ChipTechnologyToggle();
					countDownTime = Duration;
				}
			}
			else if (sp.Type == SpecialType.SpeedUp)
			{
			}
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
					if (commonGasItem.Level == 1 && !HadEffect && (float)countDownTime > 0f)
					{
						Toggle();
					}
					break;
				case ItemCallbackType.BREAK:
					if (commonGasItem.Level == 1 && HadEffect)
					{
						Stop();
					}
					break;
			}
		}

		public override void Toggle()
		{
			base.Toggle();
			if (carState != null && !HadEffect)
			{
				Float[] engineForces = carState.view.carModel.EngineForces;
				engineForces[1] = (float)engineForces[1] + (float)(int)_exForce;
				HadEffect = true;
				Debug.Log(string.Concat("动力增加", _exForce, " 至 ", carState.view.carModel.EngineForces[1]));
			}
		}

		public override void Stop()
		{
			base.Stop();
			if (carState != null && HadEffect)
			{
				Float[] engineForces = carState.view.carModel.EngineForces;
				engineForces[1] = (float)engineForces[1] - (float)(int)_exForce;
				HadEffect = false;
				Debug.Log(string.Concat("动力减少", _exForce, " 至 ", carState.view.carModel.EngineForces[1]));
			}
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
			IsEffecting = (float)countDownTime > 0f;
		}
	}
}
