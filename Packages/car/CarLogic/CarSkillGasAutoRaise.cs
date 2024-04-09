using System;
using UnityEngine;

namespace CarLogic
{
	public class CarSkillGasAutoRaise : CarSkillBase
	{
		public Float RaiseValue { get; set; }

		public override CarSkillId ID => CarSkillId.GAS_UP_AUTO;

		public override void OnActiveChange(bool active)
		{
			if (carState != null)
			{
				CarView view = carState.view;
				view.OnUpdate = (Action)Delegate.Remove(view.OnUpdate, new Action(onUpdate));
				if (active)
				{
					CarView view2 = carState.view;
					view2.OnUpdate = (Action)Delegate.Combine(view2.OnUpdate, new Action(onUpdate));
				}
			}
		}

		public override void Init(int param1, int param2, int param3)
		{
			RaiseValue = (float)param1 * 0.0001f * RaceDefines.MAX_GAS_VALUE;
		}

		public override bool Usable()
		{
			return true;
		}

		public override void Toggle()
		{
			carState.view.N2Scraper.RaiseScrapeValue(Time.deltaTime * (float)RaiseValue);
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
			if (Usable())
			{
				Toggle();
			}
		}
	}
}
