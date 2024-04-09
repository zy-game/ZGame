using System;
using UnityEngine;

namespace CarLogic
{
	public abstract class CarSkillBase
	{
		protected bool active;

		public Action<CarSkillId, int> OnTechnologySkillToggle;

		protected CarState carState;

		public abstract CarSkillId ID { get; }

		public bool Active
		{
			get
			{
				return active;
			}
			set
			{
				OnActiveChange(value);
				active = value;
			}
		}

		public CarState User
		{
			get
			{
				return carState;
			}
			set
			{
				carState = value;
			}
		}

		public abstract void OnActiveChange(bool active);

		public abstract void Init(int param1, int param2, int param3);

		public virtual bool Usable()
		{
			return false;
		}

		public virtual void Toggle()
		{
			if (carState != null && carState.CallBacks.OnSkillToggle != null)
			{
				carState.CallBacks.OnSkillToggle(carState, ID);
			}
		}

		public virtual void Stop()
		{
			if (carState != null && carState.CallBacks.OnSkillStop != null)
			{
				carState.CallBacks.OnSkillStop(carState, ID);
			}
		}

		public virtual void Break()
		{
			Stop();
		}

		protected int getRandomNum()
		{
			return (int)(Time.realtimeSinceStartup * 10000f) % 100;
		}
	}
}
