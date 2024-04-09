using System;

namespace CarLogic
{
	public class CarSkillFasterSmallGas : CarSkillBase
	{
		private bool _hadEffect;

		private Float _percent = 0;

		private Int _exForce = 0;

		public override CarSkillId ID => CarSkillId.SMALL_GAS_FASTER;

		public override void Init(int param1, int param2, int param3)
		{
			_percent = (float)param1 * 0.01f;
			_exForce = param2;
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
			return (float)getRandomNum() < (float)_percent && flag;
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

		public override void Toggle()
		{
			base.Toggle();
			if (carState != null && !_hadEffect)
			{
				Float[] engineForces = carState.view.carModel.EngineForces;
				engineForces[2] = (float)engineForces[2] + (float)(int)_exForce;
				_hadEffect = true;
			}
		}

		public override void Stop()
		{
			base.Stop();
			if (carState != null && _hadEffect)
			{
				Float[] engineForces = carState.view.carModel.EngineForces;
				engineForces[2] = (float)engineForces[2] - (float)(int)_exForce;
				_hadEffect = false;
			}
		}
	}
}
