using System;
using UnityEngine;

namespace CarLogic
{
	public class CarSkillItemNoToYes : CarSkillBase
	{
		public Float TimeInterval { get; set; }

		protected Float PassTime { get; set; }

		protected Bool IsGameStart { get; set; }

		public override CarSkillId ID => CarSkillId.ITEM_GET_NOTOYES;

		public override void OnActiveChange(bool active)
		{
			if (carState != null)
			{
				CarView view = carState.view;
				view.OnUpdate = (Action)Delegate.Remove(view.OnUpdate, new Action(update));
				CarCallBack callBacks = carState.CallBacks;
				callBacks.OnEnableMovement = (Action<bool>)Delegate.Remove(callBacks.OnEnableMovement, new Action<bool>(onEnableMovement));
				CarCallBack callBacks2 = carState.CallBacks;
				callBacks2.OnToggleItemBox = (ModifyAction<CarState, RaceItemId>)Delegate.Remove(callBacks2.OnToggleItemBox, new ModifyAction<CarState, RaceItemId>(onToggleItemBox));
				if (active)
				{
					CarView view2 = carState.view;
					view2.OnUpdate = (Action)Delegate.Combine(view2.OnUpdate, new Action(update));
					CarCallBack callBacks3 = carState.CallBacks;
					callBacks3.OnEnableMovement = (Action<bool>)Delegate.Combine(callBacks3.OnEnableMovement, new Action<bool>(onEnableMovement));
					CarCallBack callBacks4 = carState.CallBacks;
					callBacks4.OnToggleItemBox = (ModifyAction<CarState, RaceItemId>)Delegate.Combine(callBacks4.OnToggleItemBox, new ModifyAction<CarState, RaceItemId>(onToggleItemBox));
				}
			}
		}

		public override void Init(int param1, int param2, int param3)
		{
			TimeInterval = (float)param1 * 0.01f;
			IsGameStart = false;
			PassTime = 0f;
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
			if (!IsGameStart)
			{
				return false;
			}
			if ((float)PassTime < (float)TimeInterval)
			{
				return false;
			}
			return true;
		}

		public override void Toggle()
		{
			base.Toggle();
			PassTime = 0f;
			carState.view.AddItem(RaceItemFactory.BuildItemById(RaceItemFactory.GetIdRandom()));
			if (carState.CallBacks.OnItemsChange != null)
			{
				carState.CallBacks.OnItemsChange(carState);
			}
		}

		protected void onToggleItemBox(CarState car, ref RaceItemId id)
		{
			if (car.view.PlayerInfo.WorldId == carState.view.PlayerInfo.WorldId)
			{
				PassTime = 0f;
			}
		}

		protected void onEnableMovement(bool enable)
		{
			IsGameStart = enable;
		}

		protected void update()
		{
			if ((bool)IsGameStart)
			{
				PassTime = (float)PassTime + Time.deltaTime;
			}
			if (Usable())
			{
				Toggle();
			}
		}
	}
}
