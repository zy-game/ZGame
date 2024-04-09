using System;
using UnityEngine;

namespace CarLogic
{
	public class CarSkillGasAutoAddForChip : CarChipTechnologySkillBase
	{
		public Float Probability = 0;

		public int Duration;

		private bool didFirstLapFinish;

		public Float RaiseValue { get; set; }

		public override CarSkillId ID => CarSkillId.AddGas;

		public override void OnActiveChange(bool active)
		{
			if (carState != null)
			{
				CarCallBack callBacks = carState.CallBacks;
				callBacks.OnLapFinish = (Action<CarState>)Delegate.Remove(callBacks.OnLapFinish, new Action<CarState>(OnLapFinish));
				if (active)
				{
					CarCallBack callBacks2 = carState.CallBacks;
					callBacks2.OnLapFinish = (Action<CarState>)Delegate.Combine(callBacks2.OnLapFinish, new Action<CarState>(OnLapFinish));
				}
			}
		}

		public override void Init(int param1, int param2, int param3)
		{
			Probability = param2;
		}

		public override bool Usable()
		{
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

		private void OnLapFinish(CarState carState)
		{
			didFirstLapFinish = ((carState.PastLapCount == 1) ? true : false);
			if (Usable() && SkillUsable() && carState.Rank >= 3 && carState.Rank <= 5 && didFirstLapFinish)
			{
				ChipTechnologyToggle();
				Toggle();
				Debug.Log("完成第1圈赛道时处于4-6名，有Y%概率额外获得一个氮气。");
			}
		}
	}
}
