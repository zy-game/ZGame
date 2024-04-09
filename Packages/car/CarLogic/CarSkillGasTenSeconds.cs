using System;

namespace CarLogic
{
	public class CarSkillGasTenSeconds : CarSkillBase
	{
		public Float Probability { get; set; }

		public override CarSkillId ID => CarSkillId.GAS_ADD_TENSECOND;

		public override void OnActiveChange(bool active)
		{
			if (carState != null)
			{
				CarCallBack callBacks = carState.CallBacks;
				callBacks.OnPostCountdown = (Action)Delegate.Remove(callBacks.OnPostCountdown, new Action(onPostCountdown));
				if (active)
				{
					CarCallBack callBacks2 = carState.CallBacks;
					callBacks2.OnPostCountdown = (Action)Delegate.Combine(callBacks2.OnPostCountdown, new Action(onPostCountdown));
				}
			}
		}

		public override void Init(int param1, int param2, int param3)
		{
			Probability = (float)param1 * 0.01f;
		}

		public override bool Usable()
		{
			if (carState == null)
			{
				return false;
			}
			if (carState.Items.Count >= 2)
			{
				return false;
			}
			if (carState.ReachedEnd)
			{
				return false;
			}
			return (float)getRandomNum() < (float)Probability;
		}

		public override void Toggle()
		{
			base.Toggle();
			carState.view.AddItem(RaceItemFactory.BuildItemById(RaceItemId.GAS));
			if (carState.CallBacks.OnItemsChange != null)
			{
				carState.CallBacks.OnItemsChange(carState);
			}
		}

		protected void onPostCountdown()
		{
			if (Usable())
			{
				Toggle();
				carState.view.CallDelay(delegate
				{
					Stop();
				}, 1f);
			}
		}
	}
}
