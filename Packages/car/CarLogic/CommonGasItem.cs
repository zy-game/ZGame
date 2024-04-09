using System;
using System.Collections.Generic;
using UnityEngine;

namespace CarLogic
{
	public class CommonGasItem : RaceItemBase
	{
		public enum AdditionalGasType
		{
			None,
			SpeedUp,
			CupidGas
		}

		protected RaceItemId _itemId = RaceItemId.GAS;

		protected int level;

		private Vector3 totalForce = Vector3.zero;

		private EngineState CarEngineState;

		public float duration = 5f;

		private float startTime;

		public bool IsTeam;

		private bool isShaking;

		protected bool _useShake = true;

		private AdditionalGasType _gasType;

		private float _additionalMaxSpeed;

		private float _additionalEngineForce;

		private AnimationCurve _additionalEngineCurve;

		public int SuperGasLevel = 1;

		private Action<CommonGasItem> _breakCallback;

		public override RaceItemId ItemId => _itemId;

		public AdditionalGasType GasType
		{
			get
			{
				return _gasType;
			}
			set
			{
				_gasType = value;
			}
		}

		public float AdditionalMaxSpeed
		{
			get
			{
				return _additionalMaxSpeed;
			}
			set
			{
				_additionalMaxSpeed = value;
			}
		}

		public float AdditionalEngineForce
		{
			get
			{
				return _additionalEngineForce;
			}
			set
			{
				_additionalEngineForce = value;
			}
		}

		public AnimationCurve AdditionalEngineCurve
		{
			get
			{
				return _additionalEngineCurve;
			}
			set
			{
				_additionalEngineCurve = value;
			}
		}

		public float StartTime => startTime;

		public int Level => level;

		public Action<CommonGasItem> BreakCallback
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

		public void SetItemId(RaceItemId id)
		{
			_itemId = id;
		}

		public CommonGasItem(int level = 1, float duration = 4f, bool useShake = true)
		{
			this.level = level;
			this.duration = duration;
			_useShake = useShake;
		}

		private void InitAdditionalForce(CarView view)
		{
			if (_gasType == AdditionalGasType.SpeedUp)
			{
				_additionalEngineForce = SpeedUpConfig.EngineForce;
				_additionalEngineCurve = SpeedUpConfig.EngineCurve;
				_additionalMaxSpeed = SpeedUpConfig.CalculateTopSpeed(view);
			}
			else if (_gasType == AdditionalGasType.CupidGas)
			{
				_additionalEngineForce = CupidGasConfig.EngineForce;
				_additionalEngineCurve = CupidGasConfig.EngineCurve;
				_additionalMaxSpeed = CupidGasConfig.CalculateTopSpeed(view);
			}
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

		private void DoToggle()
		{
			if (_gasType != 0)
			{
				InitAdditionalForce(carState.view);
			}
			if (carState.CarPlayerType == PlayerType.PLAYER_SELF)
			{
				CarEngineState = EngineState.NORMAL;
				if (targets != null)
				{
					targets = targets;
				}
				SetN2ForceState();
				CarCallBack callBacks = carState.CallBacks;
				callBacks.OnApplyForce = (Action)Delegate.Combine(callBacks.OnApplyForce, new Action(update));
			}
			else
			{
				SetN2ForceState();
				CarView view = carState.view;
				view.OnUpdate = (Action)Delegate.Combine(view.OnUpdate, new Action(otherUpdate));
			}
			if (carState != null && carState.CallBacks.OnAffectedByItem != null)
			{
				carState.CallBacks.OnAffectedByItem(ItemId, carState, ItemCallbackType.AFFECT, this);
			}
			if (_gasType != AdditionalGasType.CupidGas)
			{
				N2StateGasType n2StateGasType = N2StateGasType.NORMAL;
				switch (SuperGasLevel)
				{
					case 2:
						n2StateGasType = N2StateGasType.LEVEL_TWO;
						break;
					case 3:
						n2StateGasType = N2StateGasType.LEVEL_THREE;
						break;
				}
				carState.N2State.GasType = (IsTeam ? N2StateGasType.TEAM : n2StateGasType);
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
			bool r = true;
			if (carState != null && carState.CallBacks.AffectChecker != null)
			{
				CarCallBack callBacks = carState.CallBacks;
				callBacks.AffectChecker = (ModifyAction<RaceItemId, bool>)Delegate.Combine(callBacks.AffectChecker, new ModifyAction<RaceItemId, bool>(GasChecker));
				carState.CallBacks.AffectChecker(ItemId, ref r);
			}
			if (!r)
			{
				Break();
				return;
			}
			List<RaceItemBase> list = new List<RaceItemBase>(4);
			carState.view.ItController.ApplyingItem(ItemId, list);
			if (list.Count == 1)
			{
				list[0].Enhance(duration);
				CommonGasItem commonGasItem = list[0] as CommonGasItem;
				if (commonGasItem != this)
				{
					if (level != 1)
					{
						Break();
						return;
					}
					commonGasItem.Break();
				}
			}
			else if (list.Count > 1)
			{
				Debug.LogWarning("Wrong gas item count :" + list.Count);
				for (int i = 0; i < list.Count; i++)
				{
					RaceItemBase raceItemBase = list[i];
					raceItemBase.Break();
				}
			}
			isShaking = false;
			if (((_useShake && carState.view.ShakeController != null) || carState.ApplyingSpecialType == SpecialType.Translate) && level > 0)
			{
				isShaking = true;
				carState.view.ShakeController.StartSpeedUpShake(level, null);
			}
			DoToggle();
		}

		public override void Break()
		{
			base.Break();
			if (carState != null)
			{
				carState.view.StopCallFunc(DoToggle);
				if (isShaking && carState.view.ShakeController != null)
				{
					isShaking = false;
					carState.view.ShakeController.EndSpeedUpShake(0.2f);
				}
				if (carState != null && carState.CallBacks.AffectChecker != null)
				{
					CarCallBack callBacks = carState.CallBacks;
					callBacks.AffectChecker = (ModifyAction<RaceItemId, bool>)Delegate.Remove(callBacks.AffectChecker, new ModifyAction<RaceItemId, bool>(GasChecker));
				}
				if (carState.CarPlayerType == PlayerType.PLAYER_SELF)
				{
					CarCallBack callBacks2 = carState.CallBacks;
					callBacks2.OnApplyForce = (Action)Delegate.Remove(callBacks2.OnApplyForce, new Action(update));
				}
				else if (carState.view != null)
				{
					CarView view = carState.view;
					view.OnUpdate = (Action)Delegate.Remove(view.OnUpdate, new Action(otherUpdate));
				}
				if (!carState.view.ItController.ApplyingExcept(this))
				{
					if (CarEngineState == EngineState.N2FORCE && carState.CallBacks.OnGas != null)
					{
						carState.CallBacks.OnGas(CarEventState.EVENT_END);
					}
					carState.N2State.Level = 0;
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

		public override void SendRequest(ItemParams ps)
		{
			base.SendRequest(ps);
		}

		protected virtual void update()
		{
			if (carState == null || carState.view == null)
			{
				return;
			}
			if (Usable(itemParams))
			{
				ApplyN2Force();
				carState.TotalForces += totalForce;
			}
			else
			{
				Break();
			}
			if (_gasType != AdditionalGasType.CupidGas)
			{
				N2StateGasType n2StateGasType = N2StateGasType.NORMAL;
				switch (SuperGasLevel)
				{
					case 2:
						n2StateGasType = N2StateGasType.LEVEL_TWO;
						break;
					case 3:
						n2StateGasType = N2StateGasType.LEVEL_THREE;
						break;
				}
				carState.N2State.GasType = (IsTeam ? N2StateGasType.TEAM : n2StateGasType);
			}
		}

		private void otherUpdate()
		{
			ApplyN2Force();
		}

		protected void SetN2ForceState()
		{
			if (carState != null && !(carState.view == null))
			{
				startTime = Time.time;
				carState.N2State.Level = level;
				carState.N2State.PreLevel = level;
				if (level == 1)
				{
					carState.AnimationFlag = AnimStage.BigHighSpeed;
				}
				else
				{
					carState.AnimationFlag = AnimStage.HighSpeed;
				}
			}
		}

		private void ApplyN2Force()
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
			if (Time.time - startTime < duration)
			{
				carState.CallBacks.OnGas((CarEngineState != 0) ? CarEventState.EVENT_DOING : CarEventState.EVENT_BEGIN);
				CarEngineState = EngineState.N2FORCE2;
				if (carState.CarPlayerType != 0 || carState.CollisionDirection != 0)
				{
					return;
				}
				if (level == 1 && carState.N2State.Level != level)
				{
					carState.N2State.Level = level;
				}
				if (_gasType == AdditionalGasType.None)
				{
					float num = 0f;
					switch (SuperGasLevel)
					{
						case 1:
							num = RaceConfig.GAS_LEVEL_ONE_POWER;
							break;
						case 2:
							num = RaceConfig.GAS_LEVEL_TWO_POWER;
							break;
						case 3:
							num = RaceConfig.GAS_LEVEL_THREE_POWER;
							break;
					}
					totalForce = (CarController.CalculateEngineForce(carModel, carState.relativeVelocity.z / (float)carModel.MaxSpeeds[level], level) + num) * Time.deltaTime * carState.ThrottleDirection;
				}
				else
				{
					totalForce = CarController.CalculateEngineForce(_additionalEngineForce, _additionalEngineCurve, carState.relativeVelocity.z / _additionalMaxSpeed) * Time.deltaTime * carState.ThrottleDirection;
				}
				if (carState.Throttle == 0f && carState.LinearVelocity < 0.3f)
				{
					totalForce += (float)RaceConfig.TimeFactor * CarController.CalculateEngineForce(carModel, carState.SpeedRatio, 0) * carState.ThrottleDirection;
				}
				if (carState.Throttle == -1f)
				{
					startTime = 0f;
				}
			}
			else
			{
				Break();
			}
		}

		public override void Enhance(float addition)
		{
			base.Enhance(addition);
			startTime = Time.time + addition - duration;
		}

		private void GasChecker(RaceItemId id, ref bool res)
		{
			if (res && (id == RaceItemId.GAS || id == RaceItemId.GasLevelTwo || id == RaceItemId.GasLevelThree || id == RaceItemId.GROUP_GAS || id == RaceItemId.COUPLE_GAS))
			{
				if (level == 1)
				{
					res = !isShaking;
				}
				else
				{
					res = true;
				}
			}
		}
	}
}
