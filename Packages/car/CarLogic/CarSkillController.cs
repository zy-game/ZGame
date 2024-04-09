using System;

namespace CarLogic
{
	public class CarSkillController : ControllerBase
	{
		public CarState carState;

		public void Init(CarState state)
		{
			carState = state;
			CarCallBack callBacks = carState.CallBacks;
			callBacks.OnReachEnd = (Action<bool>)Delegate.Combine(callBacks.OnReachEnd, new Action<bool>(OnReachEnd));
		}

		public void OnReachEnd(bool isEnd)
		{
			if (isEnd)
			{
				CarCallBack callBacks = carState.CallBacks;
				callBacks.OnReachEnd = (Action<bool>)Delegate.Remove(callBacks.OnReachEnd, new Action<bool>(OnReachEnd));
				base.Active = false;
			}
		}

		public override void OnActiveChange(bool active)
		{
			base.OnActiveChange(active);
			if (carState != null)
			{
				for (int i = 0; i < carState.Skills.Count; i++)
				{
					CarSkillBase carSkillBase = carState.Skills[i];
					carSkillBase.User = carState;
					carSkillBase.Active = active;
				}
			}
		}
	}
}
