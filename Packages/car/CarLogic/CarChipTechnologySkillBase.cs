using System;

namespace CarLogic
{
	public abstract class CarChipTechnologySkillBase : CarSkillBase
	{
		public int ChipItemId;

		public bool IsEffecting;

		public bool HadEffect;

		private float CD;

		private float leftCD;

		private int Times;

		private int leftTimes;

		private DateTime lastTime;

		protected float commonGasItemDuration = 5f;

		public override CarSkillId ID => CarSkillId.ChipTechnologySkill;

		public virtual void Init2(int Id, int times, float cd)
		{
			ChipItemId = Id;
			CD = cd;
			Times = times;
			leftCD = cd;
			leftTimes = times;
		}

		public virtual void ChipTechnologyToggle()
		{
			leftTimes--;
			lastTime = DateTime.Now;
			if (carState != null && carState.CallBacks.OnChipTechnologySkillToggle != null)
			{
				carState.CallBacks.OnChipTechnologySkillToggle(carState, this);
			}
		}

		public bool TimesUsable()
		{
			if (Times != 0)
			{
				return leftTimes > 0;
			}
			return true;
		}

		public bool CDUsable()
		{
			DateTime now = DateTime.Now;
			DateTime value = lastTime;
			if (CD != 0f)
			{
				return now.Subtract(value).TotalSeconds > (double)leftCD;
			}
			return true;
		}

		public bool SkillUsable()
		{
			if (CDUsable())
			{
				return TimesUsable();
			}
			return false;
		}
	}
}
