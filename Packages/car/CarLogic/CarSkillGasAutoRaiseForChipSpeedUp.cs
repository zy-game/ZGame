using System;
using UnityEngine;

namespace CarLogic
{
	public class CarSkillGasAutoRaiseForChipSpeedUp : CarChipTechnologySkillBase
	{
		public Float Duration;

		private Float countDownTime = 0;

		private float lastTime;

		private bool isSpeedingUp;

		public Float Probability { get; set; }

		public Float RaiseValue { get; set; }

		public override CarSkillId ID => CarSkillId.GasRaiseSpeedUp;

		public override void OnActiveChange(bool active)
		{
			if (carState != null)
			{
				CarCallBack callBacks = carState.CallBacks;
				callBacks.OnSpecialCallback = (Action<CarState, SpecialTriggerBase, SpecialCallback>)Delegate.Remove(callBacks.OnSpecialCallback, new Action<CarState, SpecialTriggerBase, SpecialCallback>(onSpecialCallback));
				CarView view = carState.view;
				view.OnUpdate = (Action)Delegate.Remove(view.OnUpdate, new Action(onUpdate));
				if (active)
				{
					CarCallBack callBacks2 = carState.CallBacks;
					callBacks2.OnSpecialCallback = (Action<CarState, SpecialTriggerBase, SpecialCallback>)Delegate.Combine(callBacks2.OnSpecialCallback, new Action<CarState, SpecialTriggerBase, SpecialCallback>(onSpecialCallback));
					CarView view2 = carState.view;
					view2.OnUpdate = (Action)Delegate.Combine(view2.OnUpdate, new Action(onUpdate));
				}
			}
		}

		public override void Init(int param1, int param2, int param3)
		{
			RaiseValue = (float)param1 * 0.01f * 0.01f * RaceDefines.MAX_GAS_VALUE;
			Probability = param2;
			Duration = param3;
		}

		public override bool Usable()
		{
			return (float)getRandomNum() < (float)Probability;
		}

		public override void Toggle()
		{
			base.Toggle();
			carState.view.N2Scraper.RaiseScrapeValue(RaiseValue);
			if (carState.view.N2Scraper.ScrapeValue >= RaceDefines.MAX_GAS_VALUE && carState.CurDriftState.Stage == DriftStage.NONE && carState.CallBacks.OnGasFull != null)
			{
				carState.CallBacks.OnGasFull();
			}
			if (carState.CallBacks.OnUpdateScrapeValue != null)
			{
				carState.CallBacks.OnUpdateScrapeValue(carState);
			}
		}

		protected void onUpdate()
		{
			if ((float)countDownTime > 0f)
			{
				countDownTime = (float)countDownTime - Time.deltaTime;
				if (Time.time - lastTime >= 1f)
				{
					Toggle();
					lastTime = Time.time;
				}
			}
			else
			{
				countDownTime = 0;
			}
		}

		protected void onSpecialCallback(CarState car, SpecialTriggerBase sp, SpecialCallback type)
		{
			Debug.Log("触发加速带");
			if (car.view.CarPlayerType != 0)
			{
				return;
			}
			if (sp.Type == SpecialType.SpeedUp && type == SpecialCallback.Toggle)
			{
				isSpeedingUp = true;
				if (Usable() && SkillUsable() && (float)countDownTime <= 0f)
				{
					lastTime = Time.time;
					ChipTechnologyToggle();
					countDownTime = Duration;
					Toggle();
				}
			}
			else if (sp.Type == SpecialType.SpeedUp && type != 0)
			{
				isSpeedingUp = false;
			}
		}
	}
}
