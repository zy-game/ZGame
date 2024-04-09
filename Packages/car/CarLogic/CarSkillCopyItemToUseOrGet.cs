using System;
using System.Collections.Generic;

namespace CarLogic
{
	public class CarSkillCopyItemToUseOrGet : CarChipTechnologySkillBase
	{
		private bool bGet = true;

		private List<RaceItemId> useIdList = new List<RaceItemId>();

		private List<RaceItemId> getIdList = new List<RaceItemId>();

		public Float Probability;

		public override CarSkillId ID => CarSkillId.ITEM_CPOY;

		public override void Init(int param1, int param2, int param3)
		{
			Probability = param2;
		}

		public override bool Usable()
		{
			return (float)getRandomNum() < (float)Probability;
		}

		public virtual void SetExParam(List<RaceItemId> useIds, List<RaceItemId> getIds, bool isGet)
		{
			useIdList = useIds;
			getIdList = getIds;
			bGet = isGet;
		}

		public RaceItemId GetId()
		{
			Random random = new Random();
			int index = random.Next(0, getIdList.Count);
			return getIdList[index];
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
				callBacks.OnUseItem = (Action<RaceItemBase>)Delegate.Remove(callBacks.OnUseItem, new Action<RaceItemBase>(onUseItem));
				if (active)
				{
					CarCallBack callBacks2 = carState.CallBacks;
					callBacks2.OnUseItem = (Action<RaceItemBase>)Delegate.Combine(callBacks2.OnUseItem, new Action<RaceItemBase>(onUseItem));
				}
			}
		}

		private void onUseItem(RaceItemBase item)
		{
			if ((useIdList.Count != 0 && !useIdList.Contains(item.ItemId)) || !Usable() || !SkillUsable())
			{
				return;
			}
			RaceItemId id = ((getIdList.Count != 0) ? GetId() : item.ItemId);
			Toggle();
			if (bGet)
			{
				if (carState != null && carState.Items.Count < 2)
				{
					carState.view.AddItem(RaceItemFactory.BuildItemById(id));
					if (carState.CallBacks.OnItemsChange != null)
					{
						carState.CallBacks.OnItemsChange(carState);
					}
				}
			}
			else if (carState.CallBacks.OnNotifyUseItem != null)
			{
				carState.CallBacks.OnNotifyUseItem(RaceItemFactory.BuildItemById(id));
			}
		}
	}
}
