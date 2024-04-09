using System;

namespace CarLogic
{
	public class CarSkillGasAutoFULLWhenDriftEnd : CarChipTechnologySkillBase
	{
		private bool isBreak;

		public Float AutoFullMin { get; set; }

		public override CarSkillId ID => CarSkillId.GAS_ADD_FULLSOON_WhenDriftEnd;

		public override void OnActiveChange(bool active)
		{
			if (carState != null)
			{
				CarCallBack callBacks = carState.CallBacks;
				callBacks.OnDrift = (Action<CarEventState>)Delegate.Remove(callBacks.OnDrift, new Action<CarEventState>(onDrift));
				if (active)
				{
					CarCallBack callBacks2 = carState.CallBacks;
					callBacks2.OnDrift = (Action<CarEventState>)Delegate.Combine(callBacks2.OnDrift, new Action<CarEventState>(onDrift));
				}
			}
		}

		public override void Init(int param1, int param2, int param3)
		{
			AutoFullMin = (float)param2 * 0.0001f * RaceDefines.MAX_GAS_VALUE;
		}

		public override bool Usable()
		{
			if (carState == null)
			{
				return false;
			}
			if (carState.CurDriftState.Stage != 0)
			{
				return false;
			}
			if (carState.Items.Count >= 2)
			{
				return false;
			}
			if (carState.view.N2Scraper.ScrapeValue < (float)AutoFullMin)
			{
				return false;
			}
			if (carState.view.N2Scraper.ScrapeValue >= RaceDefines.MAX_GAS_VALUE)
			{
				return false;
			}
			return true;
		}

		public override void Toggle()
		{
			base.Toggle();
			carState.view.N2Scraper.SetScrapeValue(RaceDefines.MAX_GAS_VALUE + 1f);
			if (carState.CallBacks.OnGasFull != null)
			{
				carState.CallBacks.OnGasFull();
			}
			if (carState != null && carState.CallBacks.OnChipTechnologySkillToggle != null)
			{
				carState.CallBacks.OnChipTechnologySkillToggle(carState, this);
			}
		}

		protected void onDrift(CarEventState eventState)
		{
			switch (eventState)
			{
				case CarEventState.EVENT_BEGIN:
					isBreak = false;
					break;
				case CarEventState.EVENT_BREAK:
					isBreak = true;
					break;
			}
			if (eventState == CarEventState.EVENT_END && !isBreak && Usable())
			{
				isBreak = true;
				Toggle();
			}
		}
	}
}