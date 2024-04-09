using System;

namespace CarLogic
{
	public class CarSkillDefenseDevil : CarSkillBase
	{
		public Float Probability { get; set; }

		public override CarSkillId ID => CarSkillId.DEFENSE_WATER;

		public override void OnActiveChange(bool active)
		{
			if (carState != null)
			{
				CarCallBack callBacks = carState.CallBacks;
				callBacks.AffectChecker = (ModifyAction<RaceItemId, bool>)Delegate.Remove(callBacks.AffectChecker, new ModifyAction<RaceItemId, bool>(onAffectChecker));
				if (active)
				{
					CarCallBack callBacks2 = carState.CallBacks;
					callBacks2.AffectChecker = (ModifyAction<RaceItemId, bool>)Delegate.Combine(callBacks2.AffectChecker, new ModifyAction<RaceItemId, bool>(onAffectChecker));
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

		public override void Break()
		{
			base.Break();
			CarCallBack callBacks = carState.CallBacks;
			callBacks.AffectChecker = (ModifyAction<RaceItemId, bool>)Delegate.Remove(callBacks.AffectChecker, new ModifyAction<RaceItemId, bool>(onAffectChecker));
		}

		public void onAffectChecker(RaceItemId id, ref bool res)
		{
			if (id == RaceItemId.CONTROL_REVERT && Usable())
			{
				base.Toggle();
				res = false;
			}
		}
	}
}
