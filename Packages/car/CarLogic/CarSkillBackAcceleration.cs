using System;

namespace CarLogic
{
	public class CarSkillBackAcceleration : CarSkillBase
	{
		private Float _probability = 0;

		private Float _exPercent = 0;

		private bool _hadEffect;

		public float ExPercent => _exPercent;

		public override CarSkillId ID => CarSkillId.ACCELERATION_BACK;

		public override void Init(int param1, int param2, int param3)
		{
			_probability = (float)param1 * 0.01f;
			_exPercent = (float)param2 * 0.0001f;
			_hadEffect = false;
		}

		public override void OnActiveChange(bool active)
		{
			if (carState != null)
			{
				CarCallBack callBacks = carState.CallBacks;
				callBacks.OnSetDeltaAcceleration = (ModifyAction<float>)Delegate.Remove(callBacks.OnSetDeltaAcceleration, new ModifyAction<float>(onSetDeltaAcceleration));
				if (active)
				{
					CarCallBack callBacks2 = carState.CallBacks;
					callBacks2.OnSetDeltaAcceleration = (ModifyAction<float>)Delegate.Combine(callBacks2.OnSetDeltaAcceleration, new ModifyAction<float>(onSetDeltaAcceleration));
				}
			}
		}

		public override bool Usable()
		{
			return (float)getRandomNum() < (float)_probability;
		}

		public override void Break()
		{
			base.Break();
			if (carState != null)
			{
				CarCallBack callBacks = carState.CallBacks;
				callBacks.OnSetDeltaAcceleration = (ModifyAction<float>)Delegate.Remove(callBacks.OnSetDeltaAcceleration, new ModifyAction<float>(onSetDeltaAcceleration));
			}
		}

		private void onSetDeltaAcceleration(ref float value)
		{
			if (Math.Abs(value) < 1E-05f)
			{
				base.Stop();
			}
			else if (Usable())
			{
				base.Toggle();
				if (carState != null)
				{
					value *= _exPercent;
				}
			}
		}
	}
}
