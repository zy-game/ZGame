using System;

namespace CarLogic
{
	public class CarSkillLongerSmallGas : CarSkillBase
	{
		private bool _hadEffect;

		private Float _probability = 0;

		private Float _exTime = 0;

		public override CarSkillId ID => CarSkillId.SMALL_GAS_LONGER;

		public override void Init(int param1, int param2, int param3)
		{
			_probability = (float)param1 * 0.01f;
			_exTime = (float)param2 * 0.0001f;
			_hadEffect = false;
		}

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

		public override bool Usable()
		{
			bool flag = true;
			for (int i = 0; i < carState.TechnologySkills.Count; i++)
			{
				CarChipTechnologySkillBase carChipTechnologySkillBase = carState.TechnologySkills[i];
				if ((carChipTechnologySkillBase.ID == CarSkillId.FastSpeed || carChipTechnologySkillBase.ID == CarSkillId.DragDirft || carChipTechnologySkillBase.ID == CarSkillId.StartQte) && carChipTechnologySkillBase.IsEffecting)
				{
					flag = false;
					break;
				}
			}
			return (float)getRandomNum() < (float)_probability && flag;
		}

		private void onAffectByItem(RaceItemId id, CarState state, ItemCallbackType type, object obj)
		{
			if (!(obj is CommonGasItem commonGasItem))
			{
				return;
			}
			switch (type)
			{
				case ItemCallbackType.AFFECT:
					if (commonGasItem.Level == 2 && Usable() && !_hadEffect)
					{
						commonGasItem.duration += _exTime;
						Toggle();
					}
					break;
				case ItemCallbackType.BREAK:
					if (commonGasItem.Level == 2 && _hadEffect)
					{
						Stop();
					}
					break;
			}
		}

		public override void Break()
		{
			Stop();
			base.Break();
		}
	}
}
