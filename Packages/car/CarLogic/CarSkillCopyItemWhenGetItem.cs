using System;

namespace CarLogic
{
	public class CarSkillCopyItemWhenGetItem : CarChipTechnologySkillBase
	{
		public Float Probability { get; set; }

		public override CarSkillId ID => CarSkillId.ITEM_COPT_GET_ITEM;

		public override void Init(int param1, int param2, int param3)
		{
			Probability = param2;
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
			return (float)getRandomNum() < (float)Probability;
		}

		public override void Toggle()
		{
			base.Toggle();
			base.ChipTechnologyToggle();
		}

		public override void OnActiveChange(bool active)
		{
			if (carState != null)
			{
				CarCallBack callBacks = carState.CallBacks;
				callBacks.OnGetItem = (Action<RaceItemBase>)Delegate.Remove(callBacks.OnGetItem, new Action<RaceItemBase>(OnGetItem));
				if (active)
				{
					CarCallBack callBacks2 = carState.CallBacks;
					callBacks2.OnGetItem = (Action<RaceItemBase>)Delegate.Combine(callBacks2.OnGetItem, new Action<RaceItemBase>(OnGetItem));
				}
			}
		}

		private void OnGetItem(RaceItemBase item)
		{
			if (Usable() && SkillUsable() && carState != null && carState.Items.Count < 2)
			{
				carState.view.AddItem(item);
				if (carState.CallBacks.OnItemsChange != null)
				{
					Toggle();
					carState.CallBacks.OnItemsChange(carState);
				}
			}
		}
	}
}
