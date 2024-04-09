using System;

namespace CarLogic
{
	public class CarSkillItemCopyUse : CarSkillBase
	{
		public Float Probability { get; set; }

		protected RaceItemId UseItem { get; set; }

		public override CarSkillId ID => CarSkillId.ITEM_GET_USE;

		public override void OnActiveChange(bool active)
		{
			if (carState != null)
			{
				CarCallBack callBacks = carState.CallBacks;
				callBacks.OnUseItem = (Action<RaceItemBase>)Delegate.Remove(callBacks.OnUseItem, new Action<RaceItemBase>(onUseItem));
				if (active)
				{
					CarCallBack callBacks2 = carState.CallBacks;
					callBacks2.OnUseItem = (Action<RaceItemBase>)Delegate.Combine(callBacks2.OnUseItem, new Action<RaceItemBase>(onUseItem));
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
			return (float)getRandomNum() < (float)Probability;
		}

		public override void Toggle()
		{
			base.Toggle();
			carState.view.AddItem(RaceItemFactory.BuildItemById(UseItem));
			if (carState.CallBacks.OnItemsChange != null)
			{
				carState.CallBacks.OnItemsChange(carState);
			}
		}

		private void onUseItem(RaceItemBase item)
		{
			UseItem = item.ItemId;
			if (Usable())
			{
				Toggle();
			}
		}
	}
}
