using System;
using UnityEngine;

namespace CarLogic
{
	public class N2ScrapeController : ControllerBase
	{
		protected CarState state;

		protected CarModel model;

		protected Vector3 lastPosition;

		protected Float scrapeValue = 0;

		protected float driftDistance;

		protected float lastSpeed;

		protected float driftStartTime;

		protected float driftStartValue;

		protected Vector3 vt;

		protected bool isInSpeedUp;

		public virtual float ScrapeValue => scrapeValue;

		public float DriftStartValue => driftStartValue;

		internal void RaiseScrapeValue(float raiseValue)
		{
			Debug.Log(" RaiseScrapeValue: " + raiseValue);
			scrapeValue = (float)scrapeValue + raiseValue;
			driftStartValue += raiseValue;
		}

		public void SetScrapeValue(float inValue)
		{
			Debug.Log(" SetScrapeValue: " + inValue);
			scrapeValue = inValue;
		}

		public virtual void Init(CarState state, CarModel model)
		{
			this.state = state;
			this.model = model;
			ResetValue();
		}

		public override void OnActiveChange(bool active)
		{
			if (state != null && model != null)
			{
				CarView view = state.view;
				CarCallBack callBacks = state.CallBacks;
				callBacks.OnDrift = (Action<CarEventState>)Delegate.Remove(callBacks.OnDrift, new Action<CarEventState>(onDrift));
				if (active && state.CarPlayerType != PlayerType.PLAYER_OTHER)
				{
					CarCallBack callBacks2 = state.CallBacks;
					callBacks2.OnDrift = (Action<CarEventState>)Delegate.Combine(callBacks2.OnDrift, new Action<CarEventState>(onDrift));
				}
			}
		}

		protected virtual void onDrift(CarEventState es)
		{
			if (base.Active && state != null && model != null)
			{
				switch (es)
				{
					case CarEventState.EVENT_BEGIN:
						onDriftStart();
						break;
					case CarEventState.EVENT_DOING:
						onDrifting();
						break;
					case CarEventState.EVENT_END:
						onDriftEnd();
						break;
					case CarEventState.EVENT_BREAK:
					case CarEventState.EVENT_GAS:
						break;
				}
			}
		}

		protected virtual void onDriftStart()
		{
			driftDistance = 0f;
			driftStartTime = Time.time;
			lastPosition = state.transform.position;
			Vector3 relativeVelocity = state.relativeVelocity;
			relativeVelocity.y = 0f;
			lastSpeed = relativeVelocity.sqrMagnitude;
			driftStartValue = scrapeValue;
			if (state.ApplyingSpecialType == SpecialType.SpeedUp)
			{
				isInSpeedUp = true;
			}
			else
			{
				isInSpeedUp = false;
			}
		}

		protected virtual void onDrifting()
		{
			float time = Time.time - driftStartTime;
			float num = scrapeValue;
			Vector3 relativeVelocity = state.relativeVelocity;
			relativeVelocity.y = 0f;
			float sqrMagnitude = relativeVelocity.sqrMagnitude;
			vt = state.transform.position;
			driftDistance = Vector3.Distance(vt, lastPosition);
			float num2 = ((float)model.EngineForces[0] * driftDistance * (float)model.GasFactor + (float)model.DfsAdjusted) * model.GasFactorDfsCurve.Evaluate(time);
			float r = ((sqrMagnitude < lastSpeed) ? (((float)model.GasFactor * (float)model.CarWeight * (lastSpeed - sqrMagnitude) * (float)RaceConfig.DriftCollectGasFactor + (float)model.DmvAdjusted) * model.GasFactorDmvCurve.Evaluate(time)) : 0f);
			if (state.ApplyingSpecialType == SpecialType.SpeedUp || isInSpeedUp)
			{
				r *= (float)RaceConfig.ScrapeValueScaleInSpeedUp;
				isInSpeedUp = true;
			}
			if (state.CallBacks.OnDriftScrapeValue != null)
			{
				state.CallBacks.OnDriftScrapeValue(r, ref r);
			}
			Debug.Log(" onDrifting: dfs= " + num2 + "      dmv= " + r);
			scrapeValue = (float)scrapeValue + (num2 + r) * RaceConfig.Gasclt_Factor;
			lastPosition = vt;
			lastSpeed = sqrMagnitude;
		}

		protected virtual void onDriftEnd()
		{
		}

		public virtual void ResetValue()
		{
			scrapeValue = 0;
			driftStartValue = 0f;
		}

		public virtual void RecoverValue()
		{
			if (state.CallBacks.OnScrapeValueRecover != null)
			{
				state.CallBacks.OnScrapeValueRecover(scrapeValue, ref driftStartValue);
			}
			scrapeValue = driftStartValue;
		}

		public virtual void RefreshDriftStartValue()
		{
			driftStartValue = scrapeValue;
		}
	}
}
