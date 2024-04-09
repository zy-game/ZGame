using System;
using UnityEngine;

namespace CarLogic
{
	public class CarSkillGasAutoRaiseForChip : CarChipTechnologySkillBase
	{
		public Float Probability = 0;

		public Float Duration;

		private bool didFirstLapFinish;

		private Float countDownTime = 0;

		public Float RaiseValue { get; set; }

		public override CarSkillId ID => CarSkillId.GasRaise;

		public override void OnActiveChange(bool active)
		{
			if (carState != null)
			{
				CarView view = carState.view;
				view.OnUpdate = (Action)Delegate.Remove(view.OnUpdate, new Action(onUpdate));
				CarCallBack callBacks = carState.CallBacks;
				callBacks.OnLapFinish = (Action<CarState>)Delegate.Remove(callBacks.OnLapFinish, new Action<CarState>(OnLapFinish));
				if (active)
				{
					CarCallBack callBacks2 = carState.CallBacks;
					callBacks2.OnLapFinish = (Action<CarState>)Delegate.Combine(callBacks2.OnLapFinish, new Action<CarState>(OnLapFinish));
					CarView view2 = carState.view;
					view2.OnUpdate = (Action)Delegate.Combine(view2.OnUpdate, new Action(onUpdate));
				}
			}
		}

		public override void Init(int param1, int param2, int param3)
		{
			RaiseValue = (float)param1 * 0.01f;
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
			if (carState.view.N2Scraper.ScrapeValue >= RaceDefines.MAX_GAS_VALUE && carState.CurDriftState.Stage == DriftStage.NONE)
			{
				if (carState.CallBacks.OnGasFull != null)
				{
					carState.CallBacks.OnGasFull();
				}
			}
			else if (carState.CallBacks.OnUpdateScrapeValue != null)
			{
				carState.CallBacks.OnUpdateScrapeValue(carState);
			}
		}

		protected void onUpdate()
		{
			if ((float)countDownTime > 0f)
			{
				Toggle();
				countDownTime = (float)countDownTime - Time.deltaTime;
			}
			else
			{
				countDownTime = 0;
			}
		}

		private void OnLapFinish(CarState carState)
		{
			didFirstLapFinish = carState.PastLapCount == 1;
			if (Usable() && SkillUsable() && carState.Rank <= 2 && didFirstLapFinish)
			{
				ChipTechnologyToggle();
				countDownTime = Duration;
				Debug.Log("完成第1圈赛道时处于1-3名，有Y%概率以每秒Z的速度获得氮气，持续X秒。");
			}
		}
	}
}
