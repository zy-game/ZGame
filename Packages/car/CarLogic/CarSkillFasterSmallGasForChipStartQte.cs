using System;
using UnityEngine;

namespace CarLogic
{
	public class CarSkillFasterSmallGasForChipStartQte : CarChipTechnologySkillBase
	{
		private Float _percent = 0;

		private Float _exForce = 0;

		private Float _duration = 0;

		private Float countDownTime = 0;

		public override CarSkillId ID => CarSkillId.StartQte;

		public override void Init(int param1, int param2, int param3)
		{
			_exForce = (float)param1 * 0.01f;
			_percent = param2;
			_duration = param3;
			HadEffect = false;
		}

		public override void OnActiveChange(bool active)
		{
			if (carState != null)
			{
				CarSkillDipatcher instance = Singleton<CarSkillDipatcher>.Instance;
				instance.OnStartQteComplete = (Action)Delegate.Remove(instance.OnStartQteComplete, new Action(OnSkillComplete));
				CarView view = carState.view;
				view.OnUpdate = (Action)Delegate.Remove(view.OnUpdate, new Action(OnUpdate));
				CarCallBack callBacks = carState.CallBacks;
				callBacks.OnAffectedByItem = (Action<RaceItemId, CarState, ItemCallbackType, object>)Delegate.Remove(callBacks.OnAffectedByItem, new Action<RaceItemId, CarState, ItemCallbackType, object>(onAffectByItem));
				if (active)
				{
					CarSkillDipatcher instance2 = Singleton<CarSkillDipatcher>.Instance;
					instance2.OnStartQteComplete = (Action)Delegate.Combine(instance2.OnStartQteComplete, new Action(OnSkillComplete));
					CarView view2 = carState.view;
					view2.OnUpdate = (Action)Delegate.Combine(view2.OnUpdate, new Action(OnUpdate));
					CarCallBack callBacks2 = carState.CallBacks;
					callBacks2.OnAffectedByItem = (Action<RaceItemId, CarState, ItemCallbackType, object>)Delegate.Combine(callBacks2.OnAffectedByItem, new Action<RaceItemId, CarState, ItemCallbackType, object>(onAffectByItem));
				}
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

		public virtual void OnSkillComplete()
		{
			if ((float)countDownTime <= 0f && Usable())
			{
				ChipTechnologyToggle();
				countDownTime = _duration;
			}
		}

		public override bool Usable()
		{
			return true;
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
					if (commonGasItem.Level == 2 && (float)countDownTime > 0f && !HadEffect)
					{
						Toggle();
					}
					break;
				case ItemCallbackType.BREAK:
					if (commonGasItem.Level == 2 && HadEffect)
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
				engineForces[2] = (float)engineForces[2] + (float)_exForce;
				HadEffect = true;
				Debug.Log("小喷动力增加效果开始");
			}
		}

		public override void Stop()
		{
			base.Stop();
			if (carState != null && HadEffect)
			{
				Float[] engineForces = carState.view.carModel.EngineForces;
				engineForces[2] = (float)engineForces[2] - (float)_exForce;
				HadEffect = false;
				Debug.Log("小喷动力增加效果结束");
			}
		}
	}
}
