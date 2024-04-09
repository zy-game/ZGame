using System;

namespace CarLogic
{
	public class CarSkillGasRaiseForChipNoCollision : CarChipTechnologySkillBase
	{
		public Float Probability = 0;

		public Float RaiseValue { get; set; }

		public override CarSkillId ID => CarSkillId.NoCollision;

		public override void OnActiveChange(bool active)
		{
			if (carState != null)
			{
				CarSkillDipatcher instance = Singleton<CarSkillDipatcher>.Instance;
				instance.OnNoCollisionComplete = (Action)Delegate.Remove(instance.OnNoCollisionComplete, new Action(OnSkillComplete));
				if (active)
				{
					CarSkillDipatcher instance2 = Singleton<CarSkillDipatcher>.Instance;
					instance2.OnNoCollisionComplete = (Action)Delegate.Combine(instance2.OnNoCollisionComplete, new Action(OnSkillComplete));
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

		private void OnSkillComplete()
		{
			if (Usable() && SkillUsable())
			{
				ChipTechnologyToggle();
				Toggle();
			}
		}
	}
}
