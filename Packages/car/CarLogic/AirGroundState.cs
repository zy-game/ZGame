using System;
using UnityEngine;

namespace CarLogic
{
	public class AirGroundState
	{
		public bool DoingGas;

		public float DROP_DOWN_HEIGHT = 0.05f;

		public float GROUND_HOLD_TIME = 0.6f;

		public float AIR_HOLD_TIME = 0.6f;

		public float AIR_GAS_EXTRAC_GRAVITY = 5f;

		public float AIR_ROTATION_TIME = 1f;

		private CarAirGroundState _currentState;

		private float _dropDownDistance;

		private float _groundPassTime;

		private float _airPassTime;

		private CarState _carState;

		private RigidbodyConstraints _normalConstraints;

		private float _normalExtracGravity;

		private Vector3 _airCarForwardTo = Vector3.zero;

		private float _preYPosition;

		private bool _airGasStarted;

		private bool _groundGasStarted;

		public void Init(CarState carState)
		{
			_carState = carState;
			_normalExtracGravity = _carState.view.carModel.ExtraGravity;
		}

		private bool CanGas()
		{
			bool result = false;
			if (_currentState == CarAirGroundState.AIR && !_airGasStarted)
			{
				result = _airPassTime <= AIR_HOLD_TIME;
			}
			else if (_currentState == CarAirGroundState.GROUND && !_groundGasStarted)
			{
				result = _groundPassTime <= GROUND_HOLD_TIME && _dropDownDistance > DROP_DOWN_HEIGHT;
			}
			return result;
		}

		private void StartAirGroundGas(Action<RaceItemBase> breakCallback = null)
		{
			AirGroundGasItem airGroundGasItem = new AirGroundGasItem(2, _carState.view.carModel.SmallN2ForceTime);
			ItemParams itemParams = new ItemParams(null, null, 0);
			itemParams.user = _carState;
			itemParams.targets = new CarState[1] { _carState };
			airGroundGasItem.BreakCallback = breakCallback;
			if (airGroundGasItem.Usable(itemParams))
			{
				airGroundGasItem.Toggle(itemParams);
			}
			else
			{
				airGroundGasItem.Break();
			}
		}

		public bool TryStartGas()
		{
			if (CanGas() && _carState != null)
			{
				if (_currentState == CarAirGroundState.AIR)
				{
					StartAirGroundGas();
					_airGasStarted = true;
				}
				else if (_currentState == CarAirGroundState.GROUND)
				{
					StartAirGroundGas();
					_groundGasStarted = true;
				}
				return _airGasStarted | _groundGasStarted;
			}
			return false;
		}

		public void UpdateAirGroundState(bool onGround)
		{
			if (!onGround)
			{
				if (_carState.view.Reseter.IsReseting)
				{
					if (_currentState != 0)
					{
						SetNoneState();
					}
				}
				else
				{
					SetAirState();
				}
			}
			else if (_currentState != 0)
			{
				SetGroundState();
			}
		}

		public void SetAirState()
		{
			if (_currentState != CarAirGroundState.AIR)
			{
				_currentState = CarAirGroundState.AIR;
				_dropDownDistance = 0f;
				_airPassTime = 0f;
				_preYPosition = _carState.transform.position.y;
				RigidbodyTool.AppendConstraints(_carState.view, RigidbodyConstraints.FreezeRotationZ);
				if (_carState.LastCorrectNode == null)
				{
					_airCarForwardTo = _carState.transform.forward;
					return;
				}
				_airCarForwardTo.x = _carState.transform.forward.x;
				_airCarForwardTo.y = _carState.LastCorrectNode.transform.forward.y;
				_airCarForwardTo.z = _carState.transform.forward.z;
				_airCarForwardTo = _airCarForwardTo.normalized;
			}
			else
			{
				UpdateAirState(_carState.transform.position.y);
			}
		}

		private void SetNoneState()
		{
			if (_currentState == CarAirGroundState.GROUND && _carState.CallBacks.OnFallDownGround != null)
			{
				_carState.CallBacks.OnFallDownGround(ItemCallbackType.BREAK);
			}
			_airGasStarted = false;
			_groundGasStarted = false;
			_currentState = CarAirGroundState.NONE;
			_airPassTime = 0f;
			_groundPassTime = 0f;
			_dropDownDistance = 0f;
			_carState.view.carModel.ExtraGravity = _normalExtracGravity;
			RigidbodyTool.SetConstraints(_carState.view, RigidbodyConstraints.None);
		}

		public void SetGroundState()
		{
			if (_currentState != CarAirGroundState.GROUND)
			{
				_airGasStarted = false;
				_currentState = CarAirGroundState.GROUND;
				_groundPassTime = 0f;
				RigidbodyTool.RemoveConstraints(_carState.view, RigidbodyConstraints.FreezeRotationZ);
				_carState.view.carModel.ExtraGravity = _normalExtracGravity;
				if (_carState.CallBacks.OnFallDownGround != null)
				{
					_carState.CallBacks.OnFallDownGround(ItemCallbackType.AFFECT);
				}
			}
			else
			{
				UpdateGroundState();
			}
		}

		private void UpdateAirState(float currentY)
		{
			_airPassTime += Time.fixedDeltaTime;
			_dropDownDistance += _preYPosition - currentY;
			_preYPosition = currentY;
			_carState.transform.forward = Vector3.Slerp(_carState.transform.forward, _airCarForwardTo, Time.fixedDeltaTime * AIR_ROTATION_TIME);
			if (_carState.N2State.Level == 1 || _airGasStarted)
			{
				_carState.view.carModel.ExtraGravity = AIR_GAS_EXTRAC_GRAVITY;
			}
		}

		private void UpdateGroundState()
		{
			_groundPassTime += Time.fixedDeltaTime;
			if (_groundPassTime > GROUND_HOLD_TIME)
			{
				SetNoneState();
			}
		}
	}
}
