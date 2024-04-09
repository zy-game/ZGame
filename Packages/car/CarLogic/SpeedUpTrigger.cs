using System;
using UnityEngine;

namespace CarLogic
{
	public class SpeedUpTrigger : SpecialTriggerBase
	{
		private static float _lastStartTime;

		private static bool _speedUping;

		private float _startTime;

		private float _duration;

		private bool _isStayBigGas;

		private float _maxSpeed;

		private CommonGasItem _speedUpGasItem;

		public override SpecialType Type => SpecialType.SpeedUp;

		public SpeedUpTrigger()
		{
			_duration = RaceConfig.SpeedUpTime;
		}

		private void ApplyForce()
		{
			if (Time.time - _startTime < _duration && target.CollisionDirection == HitWallDirection.NONE)
			{
				PlayerType carPlayerType = target.CarPlayerType;
				return;
			}
			CarCallBack callBacks = target.CallBacks;
			callBacks.OnApplyForce = (Action)Delegate.Remove(callBacks.OnApplyForce, new Action(ApplyForce));
			Stop();
			target.CallBacks.OnGas(CarEventState.EVENT_END);
		}

		public override void Stop()
		{
			base.Stop();
			CarState carState = target;
			if (carState != null && carState.CallBacks.OnAffectedByItem != null)
			{
				carState.CallBacks.OnAffectedByItem(RaceItemId.GAS, carState, ItemCallbackType.BREAK, _speedUpGasItem);
			}
		}

		public override void Toggle(CarState state)
		{
			base.Toggle(state);
			if (target != null && target.view != null && Time.time - _lastStartTime > (float)RaceConfig.SpeedUpInterval)
			{
				_maxSpeed = SpeedUpConfig.CalculateTopSpeed(target.view);
				_startTime = Time.time;
				_lastStartTime = _startTime;
				if (target.N2State.Level != 1)
				{
					ItemParams itemParams = new ItemParams(null, null, 0);
					itemParams.user = target;
					itemParams.targets = new CarState[1] { target };
					CommonGasItem commonGasItem = new CommonGasItem(1, _duration, useShake: false);
					if (commonGasItem.Usable(itemParams))
					{
						commonGasItem.GasType = CommonGasItem.AdditionalGasType.SpeedUp;
						commonGasItem.AdditionalMaxSpeed = _maxSpeed;
						commonGasItem.AdditionalEngineCurve = SpeedUpConfig.EngineCurve;
						commonGasItem.AdditionalEngineForce = SpeedUpConfig.EngineForce;
						commonGasItem.Toggle(itemParams);
					}
					_isStayBigGas = false;
				}
				else
				{
					foreach (RaceItemBase applyItem in target.ApplyItems)
					{
						if (applyItem.ItemId == RaceItemId.GAS || applyItem.ItemId == RaceItemId.GasLevelTwo || applyItem.ItemId == RaceItemId.GasLevelThree || applyItem.ItemId == RaceItemId.COUPLE_GAS)
						{
							CommonGasItem commonGasItem2 = applyItem as CommonGasItem;
							if (commonGasItem2.Level == 1)
							{
								commonGasItem2.GasType = CommonGasItem.AdditionalGasType.SpeedUp;
								commonGasItem2.AdditionalMaxSpeed = _maxSpeed;
								commonGasItem2.AdditionalEngineCurve = SpeedUpConfig.EngineCurve;
								commonGasItem2.AdditionalEngineForce = SpeedUpConfig.EngineForce;
								float num = commonGasItem2.StartTime + commonGasItem2.duration - Time.time;
								if (_duration > num)
								{
									commonGasItem2.duration += _duration - num;
									_duration = num;
								}
								if (_speedUpGasItem == null)
								{
									_speedUpGasItem = new CommonGasItem(1, _duration, useShake: false);
								}
								CarState carState = target;
								if (carState != null && carState.CallBacks.OnAffectedByItem != null)
								{
									carState.CallBacks.OnAffectedByItem(RaceItemId.GAS, carState, ItemCallbackType.TOGGLE, _speedUpGasItem);
									carState.CallBacks.OnAffectedByItem(RaceItemId.GAS, carState, ItemCallbackType.AFFECT, _speedUpGasItem);
								}
								break;
							}
						}
						else if (applyItem.ItemId == RaceItemId.CUPID_GAS)
						{
							if (_speedUpGasItem == null)
							{
								_speedUpGasItem = new CommonGasItem(1, _duration, useShake: false);
							}
							CarState carState2 = target;
							if (carState2 != null && carState2.CallBacks.OnAffectedByItem != null)
							{
								carState2.CallBacks.OnAffectedByItem(RaceItemId.GAS, carState2, ItemCallbackType.TOGGLE, _speedUpGasItem);
								carState2.CallBacks.OnAffectedByItem(RaceItemId.GAS, carState2, ItemCallbackType.AFFECT, _speedUpGasItem);
							}
							break;
						}
					}
					target.CallBacks.OnGas(CarEventState.EVENT_BEGIN);
					_isStayBigGas = true;
				}
				CarCallBack callBacks = target.CallBacks;
				callBacks.OnApplyForce = (Action)Delegate.Combine(callBacks.OnApplyForce, new Action(ApplyForce));
				if (target.CallBacks.OnSpeedUpToggle != null)
				{
					target.CallBacks.OnSpeedUpToggle();
				}
			}
			else
			{
				Stop();
			}
		}
	}
}
