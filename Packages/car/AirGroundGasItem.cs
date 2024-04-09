using System;
using CarLogic;
using UnityEngine;

public class AirGroundGasItem : RaceItemBase
{
	private int _level = 2;

	private Vector3 _totalForce = Vector3.zero;

	private float _duration;

	private float _startTime;

	private Action<RaceItemBase> _breakCallback;

	public override RaceItemId ItemId => RaceItemId.AIR_GROUND_GAS;

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

	public AirGroundGasItem(int level = 2, float duration = 2f)
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

	private void SetAirGroundGasState()
	{
		if (carState != null && !(carState.view == null))
		{
			_startTime = Time.time;
			carState.AirGround.DoingGas = true;
			carState.AnimationFlag = AnimStage.HighSpeed;
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
			SetAirGroundGasState();
			CarCallBack callBacks = carState.CallBacks;
			callBacks.OnApplyForce = (Action)Delegate.Combine(callBacks.OnApplyForce, new Action(OnSelfUpdate));
		}
		else
		{
			SetAirGroundGasState();
			CarView view = carState.view;
			view.OnUpdate = (Action)Delegate.Combine(view.OnUpdate, new Action(OnOtherUpdate));
		}
		carState.CallBacks.OnAirGroundGas(CarEventState.EVENT_BEGIN);
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
		DoToggle();
	}

	public override void Break()
	{
		base.Break();
		if (carState != null)
		{
			carState.AirGround.DoingGas = false;
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
			if (!carState.view.ItController.ApplyingExcept(this))
			{
				carState.CallBacks.OnAirGroundGas(CarEventState.EVENT_END);
				if (carState.AnimationFlag == AnimStage.HighSpeed || carState.AnimationFlag == AnimStage.BigHighSpeed)
				{
					carState.AnimationFlag = AnimStage.CommonDrive;
				}
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
			carState.CallBacks.OnAirGroundGas(CarEventState.EVENT_DOING);
			if (carState.CarPlayerType == PlayerType.PLAYER_SELF && carState.CollisionDirection == HitWallDirection.NONE)
			{
				_totalForce = CarController.CalculateEngineForce(carModel, carState.relativeVelocity.z / (float)carModel.MaxSpeeds[_level], _level) * Time.deltaTime * carState.ThrottleDirection;
				if (carState.Throttle == 0f && carState.LinearVelocity < 0.3f)
				{
					_totalForce += (float)RaceConfig.TimeFactor * CarController.CalculateEngineForce(carModel, carState.SpeedRatio, 0) * carState.ThrottleDirection;
				}
				if (carState.Throttle == -1f)
				{
					_startTime = 0f;
				}
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
				carState.TotalForces += _totalForce;
			}
			else
			{
				Break();
			}
		}
	}
}
