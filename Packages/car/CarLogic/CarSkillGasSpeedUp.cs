using System;

namespace CarLogic
{
	public class CarSkillGasSpeedUp : CarSkillBase
	{
		public Float Probability { get; set; }

		public Float RaiseValue { get; set; }

		public override CarSkillId ID => CarSkillId.GAS_UP_SPEEDUP;

		public override void OnActiveChange(bool active)
		{
			if (carState != null)
			{
				CarCallBack callBacks = carState.CallBacks;
				callBacks.OnSpeedUpToggle = (Action)Delegate.Remove(callBacks.OnSpeedUpToggle, new Action(onSpeedUpToggle));
				if (active)
				{
					CarCallBack callBacks2 = carState.CallBacks;
					callBacks2.OnSpeedUpToggle = (Action)Delegate.Combine(callBacks2.OnSpeedUpToggle, new Action(onSpeedUpToggle));
				}
			}
		}

		public override void Init(int param1, int param2, int param3)
		{
			Probability = (float)param1 * 0.01f;
			RaiseValue = (float)param2 * 0.0001f * RaceDefines.MAX_GAS_VALUE;
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

		protected void onSpeedUpToggle()
		{
			if (Usable())
			{
				Toggle();
			}
		}
	}
}
