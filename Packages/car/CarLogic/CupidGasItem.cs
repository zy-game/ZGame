using System;
using UnityEngine;

namespace CarLogic
{
	public class CupidGasItem : RaceItemBase
	{
		private int _level = 1;

		private bool _isStarted;

		private float _duration;

		private float _startTime;

		private float _maxSpeed;

		private Action<RaceItemBase> _breakCallback;

		public override RaceItemId ItemId => RaceItemId.CUPID_GAS;

		public Action<RaceItemBase> BreakCallback
		{
			get
			{
				return _breakCallback;
			}
			set
			{
				_breakCallback = value;
			}
		}

		public CupidGasItem(int level = 1, float duration = 2f)
		{
			_level = level;
			_duration = duration;
		}

		public override bool Usable(ItemParams ps)
		{
			if (ps == null)
			{
				return false;
			}
			if (ps.targets == null || ps.targets.Length == 0)
			{
				return false;
			}
			carState = ps.targets[0];
			if (carState == null || carState.transform == null)
			{
				return false;
			}
			return base.Usable(ps);
		}

		private void SetCupidGasState()
		{
			if (carState != null && !(carState.view == null))
			{
				_startTime = Time.time;
				_isStarted = true;
				carState.AnimationFlag = AnimStage.HighSpeed;
				carState.N2State.IsCupidGas = true;
			}
		}

		private void DoToggle()
		{
			if (carState.CarPlayerType == PlayerType.PLAYER_SELF)
			{
				if (targets != null)
				{
					targets = targets;
				}
				SetCupidGasState();
				if (carState.N2State.Level != 1)
				{
					ItemParams itemParams = new ItemParams(null, null, 0);
					itemParams.user = carState;
					itemParams.targets = targets;
					CommonGasItem commonGasItem = new CommonGasItem(1, _duration, useShake: false);
					commonGasItem.SetItemId(RaceItemId.NONE);
					if (commonGasItem.Usable(itemParams))
					{
						commonGasItem.GasType = CommonGasItem.AdditionalGasType.CupidGas;
						commonGasItem.AdditionalMaxSpeed = _maxSpeed;
						commonGasItem.AdditionalEngineCurve = CupidGasConfig.EngineCurve;
						commonGasItem.AdditionalEngineForce = CupidGasConfig.EngineForce;
						commonGasItem.Toggle(itemParams);
					}
				}
				else
				{
					foreach (RaceItemBase applyItem in carState.ApplyItems)
					{
						if (applyItem.ItemId != RaceItemId.GAS && applyItem.ItemId != RaceItemId.COUPLE_GAS)
						{
							continue;
						}
						CommonGasItem commonGasItem2 = applyItem as CommonGasItem;
						if (commonGasItem2.Level == 1)
						{
							commonGasItem2.GasType = CommonGasItem.AdditionalGasType.CupidGas;
							commonGasItem2.AdditionalMaxSpeed = _maxSpeed;
							commonGasItem2.AdditionalEngineCurve = CupidGasConfig.EngineCurve;
							commonGasItem2.AdditionalEngineForce = CupidGasConfig.EngineForce;
							float num = commonGasItem2.StartTime + commonGasItem2.duration - Time.time;
							if (_duration > num)
							{
								commonGasItem2.duration += _duration - num;
							}
							break;
						}
					}
					carState.CallBacks.OnGas(CarEventState.EVENT_BEGIN);
				}
				CarCallBack callBacks = carState.CallBacks;
				callBacks.OnApplyForce = (Action)Delegate.Combine(callBacks.OnApplyForce, new Action(OnSelfUpdate));
				carState.N2State.GasType = N2StateGasType.CUPID;
			}
			else
			{
				SetCupidGasState();
				CarView view = carState.view;
				view.OnUpdate = (Action)Delegate.Combine(view.OnUpdate, new Action(OnOtherUpdate));
			}
			if (carState != null && carState.CallBacks.OnAffectedByItem != null)
			{
				carState.CallBacks.OnAffectedByItem(ItemId, carState, ItemCallbackType.AFFECT, this);
			}
		}

		public override void Toggle(ItemParams ps)
		{
			base.Toggle(ps);
			if (ps.targets == null || ps.targets.Length == 0)
			{
				if (ps.user == null)
				{
					Break();
					Debug.LogWarning("Bad Toggle");
					return;
				}
				ps.targets = new CarState[1] { ps.user };
				itemParams = ps;
			}
			carState = ps.targets[0];
			targets = ps.targets;
			_maxSpeed = CupidGasConfig.CalculateTopSpeed(carState.view);
			DoToggle();
		}

		public override void Break()
		{
			base.Break();
			carState.N2State.IsCupidGas = false;
			if (carState != null)
			{
				if (carState.CarPlayerType == PlayerType.PLAYER_SELF)
				{
					CarCallBack callBacks = carState.CallBacks;
					callBacks.OnApplyForce = (Action)Delegate.Remove(callBacks.OnApplyForce, new Action(OnSelfUpdate));
				}
				else if (carState.view != null)
				{
					CarView view = carState.view;
					view.OnUpdate = (Action)Delegate.Remove(view.OnUpdate, new Action(OnOtherUpdate));
				}
				if (!carState.view.ItController.ApplyingExcept(this) && (carState.AnimationFlag == AnimStage.HighSpeed || carState.AnimationFlag == AnimStage.BigHighSpeed))
				{
					carState.AnimationFlag = AnimStage.CommonDrive;
				}
			}
			if (_breakCallback != null)
			{
				_breakCallback(this);
			}
		}

		private void ApplyGasForce()
		{
			if (carState == null || carState.view == null)
			{
				return;
			}
			if (itemParams.targets == null || itemParams.targets.Length == 0)
			{
				Break();
				return;
			}
			carState = itemParams.targets[0];
			carModel = carState.view.carModel;
			if (Time.time - _startTime < _duration)
			{
				carState.CallBacks.OnGas(CarEventState.EVENT_DOING);
				if (carState.CarPlayerType == PlayerType.PLAYER_SELF && carState.CollisionDirection == HitWallDirection.NONE && carState.Throttle == -1f)
				{
					_startTime = 0f;
				}
			}
			else
			{
				Break();
			}
		}

		private void OnOtherUpdate()
		{
			ApplyGasForce();
		}

		private void OnSelfUpdate()
		{
			if (carState != null && !(carState.view == null))
			{
				if (Usable(itemParams))
				{
					ApplyGasForce();
					carState.N2State.GasType = N2StateGasType.CUPID;
				}
				else
				{
					Break();
				}
			}
		}
	}
}
