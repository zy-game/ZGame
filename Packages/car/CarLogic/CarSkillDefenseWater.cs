using System;

namespace CarLogic
{
	public class CarSkillDefenseWater : CarSkillBase
	{
		public Float Probability { get; set; }

		public override CarSkillId ID => CarSkillId.DEFENSE_WATER;

		public override void OnActiveChange(bool active)
		{
			if (carState != null)
			{
				CarView view = carState.view;
				view.OnUpdate = (Action)Delegate.Remove(view.OnUpdate, new Action(onUpdate));
				CarCallBack callBacks = carState.CallBacks;
				callBacks.OnAffectedByItem = (Action<RaceItemId, CarState, ItemCallbackType, object>)Delegate.Remove(callBacks.OnAffectedByItem, new Action<RaceItemId, CarState, ItemCallbackType, object>(onAffectedByItem));
				if (active)
				{
					CarCallBack callBacks2 = carState.CallBacks;
					callBacks2.OnAffectedByItem = (Action<RaceItemId, CarState, ItemCallbackType, object>)Delegate.Combine(callBacks2.OnAffectedByItem, new Action<RaceItemId, CarState, ItemCallbackType, object>(onAffectedByItem));
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

		public override void Toggle()
		{
			base.Toggle();
		}

		private void onUpdate()
		{
			if (carState.GroundHit != 0)
			{
				ItemParams itemParams = new ItemParams(null, null, 0);
				itemParams.user = carState;
				itemParams.targets = new CarState[1] { carState };
				CommonGasItem commonGasItem = new CommonGasItem(2, RaceConfig.SpeedUpTime);
				if (commonGasItem.Usable(itemParams))
				{
					commonGasItem.duration = RaceConfig.SpeedUpTime;
					commonGasItem.Toggle(itemParams);
					Toggle();
				}
				CarView view = carState.view;
				view.OnUpdate = (Action)Delegate.Remove(view.OnUpdate, new Action(onUpdate));
			}
		}

		public void onAffectedByItem(RaceItemId id, CarState car, ItemCallbackType type, object obj)
		{
			if ((id == RaceItemId.WATER_BOMB || id == RaceItemId.WATER_FLY) && car.view.PlayerInfo.WorldId == carState.view.PlayerInfo.WorldId && obj is RaceItemBase raceItemBase && raceItemBase.User.view.PlayerInfo.WorldId != carState.view.PlayerInfo.WorldId && type == ItemCallbackType.BREAK && Usable())
			{
				CarView view = carState.view;
				view.OnUpdate = (Action)Delegate.Combine(view.OnUpdate, new Action(onUpdate));
			}
		}
	}
}
