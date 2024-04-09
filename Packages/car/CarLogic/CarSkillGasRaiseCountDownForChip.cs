using System;
using UnityEngine;

namespace CarLogic
{
	public class CarSkillGasRaiseCountDownForChip : CarChipTechnologySkillBase
	{
		public Float Probability = 0;

		private int Duration;

		public Float RaiseValue { get; set; }

		public override CarSkillId ID => CarSkillId.CountDownGasRaise;

		public override void OnActiveChange(bool active)
		{
			if (carState != null)
			{
				CarCallBack callBacks = carState.CallBacks;
				callBacks.OnPreCountdown = (Action)Delegate.Remove(callBacks.OnPreCountdown, new Action(OnPreCountdown));
				if (active)
				{
					CarCallBack callBacks2 = carState.CallBacks;
					callBacks2.OnPreCountdown = (Action)Delegate.Combine(callBacks2.OnPreCountdown, new Action(OnPreCountdown));
				}
			}
		}

		public override void Init(int param1, int param2, int param3)
		{
			RaiseValue = (float)param1 * 0.01f * 0.01f * RaceDefines.MAX_GAS_VALUE;
			Probability = param2;
		}

		public override bool Usable()
		{
			int randomNum = getRandomNum();
			return (float)randomNum < (float)Probability;
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

		private void OnPreCountdown()
		{
			if (Usable() && SkillUsable())
			{
				ChipTechnologyToggle();
				Toggle();
				Debug.Log("比赛开始的倒计时期间,有Y%概率额外获得Z%氮气。");
			}
		}
	}
}
