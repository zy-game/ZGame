using System;

namespace CarLogic
{
	public class CarSkillItemCopyHit : CarSkillBase
	{
		public Float Probability { get; set; }

		public RaceItemId HitItem { get; set; }

		public override CarSkillId ID => CarSkillId.ITEM_GET_BEENHIT;

		public override void OnActiveChange(bool active)
		{
			if (carState != null)
			{
				CarCallBack callBacks = carState.CallBacks;
				callBacks.OnAffectedByItem = (Action<RaceItemId, CarState, ItemCallbackType, object>)Delegate.Remove(callBacks.OnAffectedByItem, new Action<RaceItemId, CarState, ItemCallbackType, object>(onAffectByItem));
				if (active)
				{
					CarCallBack callBacks2 = carState.CallBacks;
					callBacks2.OnAffectedByItem = (Action<RaceItemId, CarState, ItemCallbackType, object>)Delegate.Combine(callBacks2.OnAffectedByItem, new Action<RaceItemId, CarState, ItemCallbackType, object>(onAffectByItem));
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
			carState.view.AddItem(RaceItemFactory.BuildItemById(HitItem));
			if (carState.CallBacks.OnItemsChange != null)
			{
				carState.CallBacks.OnItemsChange(carState);
			}
		}

		private void onAffectByItem(RaceItemId id, CarState car, ItemCallbackType type, object o)
		{
			if (car.view.PlayerInfo.WorldId == carState.view.PlayerInfo.WorldId && type == ItemCallbackType.AFFECT && o is RaceItemBase raceItemBase && raceItemBase.User.view.PlayerInfo.WorldId != carState.view.PlayerInfo.WorldId)
			{
				HitItem = id;
				if (Usable())
				{
					Toggle();
				}
			}
		}
	}
}
