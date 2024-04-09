using System;
using System.Collections.Generic;

namespace CarLogic
{
	public class CarSkillUseItemToTeamOrOppTeam : CarChipTechnologySkillBase
	{
		private bool bTeam = true;

		public Float Probability;

		private List<RaceItemId> useIdList = new List<RaceItemId>();

		public override CarSkillId ID => CarSkillId.ITEM_CPOY;

		public override void Init(int param1, int param2, int param3)
		{
			Probability = param2;
		}

		public virtual void SetExParam(List<RaceItemId> useIds, bool isTeam)
		{
			useIdList = useIds;
			bTeam = isTeam;
		}

		public override bool Usable()
		{
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
			if (Usable() && (useIdList.Count == 0 || useIdList.Contains(item.ItemId)) && SkillUsable() && carState.CallBacks.OnNotifyUseToGroup != null)
			{
				Toggle();
				carState.CallBacks.OnNotifyUseToGroup(item, bTeam);
			}
		}
	}
}
