using System;

namespace CarLogic
{
	public class CarSkillGasKeep : CarSkillBase
	{
		public Float Probability { get; set; }

		public override CarSkillId ID => CarSkillId.GAS_UP_KEEP;

		public override void OnActiveChange(bool active)
		{
			if (carState != null)
			{
				CarCallBack callBacks = carState.CallBacks;
				callBacks.OnScrapeValueRecover = (ModifyAction<float, float>)Delegate.Remove(callBacks.OnScrapeValueRecover, new ModifyAction<float, float>(onScrapeValueRecover));
				if (active)
				{
					CarCallBack callBacks2 = carState.CallBacks;
					callBacks2.OnScrapeValueRecover = (ModifyAction<float, float>)Delegate.Combine(callBacks2.OnScrapeValueRecover, new ModifyAction<float, float>(onScrapeValueRecover));
				}
			}
		}

		public override void Init(int param1, int param2, int param3)
		{
			Probability = (float)param1 * 0.01f;
		}

		public override bool Usable()
		{
			return (float)getRandomNum() < (float)Probability;
		}

		protected virtual void onScrapeValueRecover(float inValue, ref float outValue)
		{
			if (Usable())
			{
				outValue = inValue;
				base.Toggle();
				carState.view.CallDelay(delegate
				{
					Stop();
				}, 1f);
			}
			if (outValue >= RaceDefines.MAX_GAS_VALUE && carState.CurDriftState.Stage == DriftStage.NONE && carState.CallBacks.OnGasFull != null)
			{
				carState.CallBacks.OnGasFull();
			}
		}
	}
}
