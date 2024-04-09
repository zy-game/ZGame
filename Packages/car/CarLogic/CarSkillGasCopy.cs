using System;

namespace CarLogic
{
	public class CarSkillGasCopy : CarSkillBase
	{
		public Float Probability { get; set; }

		public override CarSkillId ID => CarSkillId.GAS_ADD_COPY;

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
			carState.view.AddItem(RaceItemFactory.BuildItemById(RaceItemId.GAS));
			if (carState.CallBacks.OnItemsChange != null)
			{
				carState.CallBacks.OnItemsChange(carState);
			}
		}

		private void onAffectByItem(RaceItemId id, CarState car, ItemCallbackType type, object o)
		{
			if ((id == RaceItemId.GAS || id == RaceItemId.GROUP_GAS) && o is CommonGasItem { Level: not 2 } && car.view.PlayerInfo.WorldId == carState.view.PlayerInfo.WorldId && type == ItemCallbackType.AFFECT && Usable())
			{
				Toggle();
			}
		}
	}
}
