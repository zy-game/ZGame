using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CarLogic
{
	[Serializable]
	public class CarController : ControllerBase, IResetChecker
	{
		private CarModel carModel;

		private Transform transform;

		private Rigidbody rigidbody;

		private AbstractView absView;

		private float initialDragMultiplierX = 10f;

		private bool canSteer;

		private bool canDrive;

		[SerializeField]
		private Vector3 relativeVelocity = Vector3.zero;

		[SerializeField]
		private Vector3 totalForce = Vector3.zero;

		private float totalAngle;

		public float angleSpeed = 180f;

		public AnimationCurve NormalEngineForce = AnimationCurve.Linear(0f, 1f, 1f, 1f);

		public AnimationCurve N2EngineForce = AnimationCurve.Linear(0f, 1f, 1f, 1f);

		public AnimationCurve N2EngineForce2 = AnimationCurve.Linear(0f, 1f, 1f, 1f);

		public AnimationCurve N2EngineForce3 = AnimationCurve.Linear(0f, 1f, 1f, 1f);

		public float LinearVelocity;

		public EngineState CarEngineState;

		[NonSerialized]
		[HideInInspector]
		public Float SLEEP_LINEAR_VELOCITY = 0.3f;

		[NonSerialized]
		[HideInInspector]
		public Float STEER_FACTOR = 20;

		[NonSerialized]
		[HideInInspector]
		public Float HIT_WALL_MIN_SPEED = 1;

		[NonSerialized]
		[HideInInspector]
		public Float HIT_WALL_EXTRA_STEER = 24;

		public const float DRIFT_GAS_DOT = 0.86f;

		[NonSerialized]
		[HideInInspector]
		public Float REFORWARD_FACTOR = 3.5f;

		public float MaxResetSpeed = 3f;

		public float MaxResetDistance = 1.2f;

		private static Vector3 vzero = Vector3.zero;

		private static Vector3 vup = Vector3.up;

		private static Vector3 vforward = Vector3.forward;

		private Vector3 tmpVector = Vector3.zero;

		private const float stdFriction = 0.4f;

		private WheelHit[] wheelHits;

		private Wheel[] wheels;

		private int groundState;

		private int lastGroundState;

		private CarState carState;

		private DriftController drifter = new DriftController();

		private CommonGasItem gasItem;

		private bool _waitingForSecondSmallGas;

		private float curHitDot;

		public const string RoadTag = "Road";

		public static float ResetDelayTime = 2f;

		private bool lockRotY;

		private bool noXDrag;

		private float lockYTime;

		private float noXTime;

		private float exSteerTime;

		private const float LOCK_Y_ROT_TIME = 0.1f;

		private const float NO_X_DRAG_TIME = 0.4f;

		private const float EXTRA_STEER_TIME = 0.3f;

		private static Quaternion leftExtra = Quaternion.Euler(0f, -3f, 0f);

		private static Quaternion rightExtra = Quaternion.Euler(0f, 3f, 0f);

		public bool forceUpward = true;

		private Queue<CommonGasItem> _smallGasQue;

		private static readonly float _specialStayingInterval = 0.3f;

		private float _lastSpecialStayingTime;

		private WheelFrictionCurve wfc;

		private Vector3 tmp = Vector3.zero;

		private Collider lastSpeedTrigger;

		private SpecialTriggerBase lastSustainedSpeedUp;

		private float lastSpTime;

		private static Quaternion eu90y = Quaternion.Euler(0f, 90f, 0f);

		private static Quaternion eu180y = Quaternion.Euler(0f, 180f, 0f);

		public DriftController Drifter => drifter;

		public object ResetUserData
		{
			get
			{
				if (carState.transform != null)
				{
					return carState.transform.position;
				}
				return Vector3.zero;
			}
		}

		public float ResetDelay => ResetDelayTime;

		public void Init(AbstractView view, Transform transform, CarState state, CarModel model)
		{
			carModel = model;
			this.transform = transform;
			Debug.Log(" --- CarController in CarLogic.Dll ---  Modify by LiuShibin");
			rigidbody = transform.GetComponent<Rigidbody>();
			carState = state;
			absView = view;
			drifter.Init(state);
			_smallGasQue = new Queue<CommonGasItem>();
		}

		public override void OnActiveChange(bool a)
		{
			if (a)
			{
				AbstractView abstractView = absView;
				abstractView.OnFixedupdate = (Action)Delegate.Combine(abstractView.OnFixedupdate, new Action(fixedUpdate));
				CarCallBack callBacks = carState.CallBacks;
				callBacks.OnDrift = (Action<CarEventState>)Delegate.Combine(callBacks.OnDrift, new Action<CarEventState>(onDrift));
				CarView view = carState.view;
				view.OnTriggerBegin = (Action<Collider>)Delegate.Combine(view.OnTriggerBegin, new Action<Collider>(onTriggerSpecialBegin));
				CarView view2 = carState.view;
				view2.OnTriggerEnd = (Action<Collider>)Delegate.Combine(view2.OnTriggerEnd, new Action<Collider>(onTriggerSpecialEnd));
				CarView view3 = carState.view;
				view3.OnTriggerStaying = (Action<Collider>)Delegate.Combine(view3.OnTriggerStaying, new Action<Collider>(onTriggerSpecialStaying));
			}
			else
			{
				AbstractView abstractView2 = absView;
				abstractView2.OnFixedupdate = (Action)Delegate.Remove(abstractView2.OnFixedupdate, new Action(fixedUpdate));
				CarCallBack callBacks2 = carState.CallBacks;
				callBacks2.OnDrift = (Action<CarEventState>)Delegate.Remove(callBacks2.OnDrift, new Action<CarEventState>(onDrift));
				CarView view4 = carState.view;
				view4.OnTriggerBegin = (Action<Collider>)Delegate.Remove(view4.OnTriggerBegin, new Action<Collider>(onTriggerSpecialBegin));
				CarView view5 = carState.view;
				view5.OnTriggerEnd = (Action<Collider>)Delegate.Remove(view5.OnTriggerEnd, new Action<Collider>(onTriggerSpecialEnd));
				CarView view6 = carState.view;
				view6.OnTriggerStaying = (Action<Collider>)Delegate.Remove(view6.OnTriggerStaying, new Action<Collider>(onTriggerSpecialStaying));
			}
			updateDriftState();
			lastGroundState = 15;
		}

		public void Init()
		{
			carModel.EngineCurves = new AnimationCurve[4];
			carModel.EngineCurves[0] = NormalEngineForce;
			carModel.EngineCurves[1] = N2EngineForce;
			carModel.EngineCurves[2] = N2EngineForce2;
			carModel.EngineCurves[3] = N2EngineForce3;
			int num = carModel.EngineForces.Length;
			carModel.MaxSpeeds = new Float[num];
			carModel.MaxSpeedsKmph = new Float[num];
			UpdateMaxSpeeds();
			carModel.MinDriftableSpeed = (float)carModel.MaxSpeeds[0] * carModel.MinDriftableSpeedRatio;
			carState.AirDragMultiplier = carModel.DragMultiplier;
			initialDragMultiplierX = carModel.DragMultiplier.x;
			Physics.gravity = new Vector3(0f, 0f - (float)carModel.Gravity, 0f);
		}

		public void UpdateMaxSpeeds()
		{
			for (int i = 0; i < carModel.MaxSpeeds.Length; i++)
			{
				carModel.MaxSpeeds[i] = CalculateTopSpeed(carModel, i);
				carModel.MaxSpeedsKmph[i] = 3.6f * (float)carModel.MaxSpeeds[i];
			}
		}

		public void SetDeltaAcceleration(float delta)
		{
			if (carState.CallBacks.OnSetDeltaAcceleration != null)
			{
				carState.CallBacks.OnSetDeltaAcceleration(ref delta);
			}
			carModel.DeltaAcceleration = delta;
			UpdateMaxSpeeds();
		}

		private void ontriggerEnter(Collider c)
		{
			if (base.Active && carState.CarPlayerType == PlayerType.PLAYER_SELF && c.gameObject.layer == 9 && carState.CollisionDirection == HitWallDirection.NONE && !lockRotY)
			{
				lockRotY = true;
				lockYTime = Time.time;
				carState.rigidbody.angularVelocity = Vector3.zero;
				RigidbodyTool.SetConstraints(carState.view, RigidbodyConstraints.FreezeRotationY);
			}
		}

		private void onStayDragRange(Collider col)
		{
			if (col.gameObject.layer != 9)
			{
				return;
			}
			float num = 4f;
			if (col.Raycast(new Ray(rigidbody.position, rigidbody.velocity), out var hitInfo, num))
			{
				float num2 = Vector3.Dot(hitInfo.normal, rigidbody.velocity);
				if (!(num2 >= 0f))
				{
					tmpVector = Vector3.Project(rigidbody.velocity, hitInfo.normal);
					float num3 = 1f - (hitInfo.point - rigidbody.position).sqrMagnitude / num / num;
					rigidbody.velocity += -6f * num3 * num3 * tmpVector;
				}
			}
		}

		public void ResizeBoxCollider()
		{
			BoxCollider componentInChildren = absView.gameObject.GetComponentInChildren<BoxCollider>();
			if ((bool)componentInChildren && (bool)SyncMoveController.OpenNewCollision)
			{
				BuildCollider(componentInChildren);
			}
		}

		private static CapsuleCollider BuildCollider(BoxCollider box)
		{
			Transform parent = box.transform.parent.parent;
			Transform transform = parent.Find("CODbody");
			if (!transform)
			{
				transform = new GameObject("CODbody").transform;
				transform.parent = parent;
				transform.position = box.transform.position;
				transform.rotation = box.transform.rotation;
				transform.localScale = Vector3.one;
				transform.gameObject.AddComponent<CapsuleCollider>();
			}
			transform.gameObject.layer = 30;
			CapsuleCollider component = transform.GetComponent<CapsuleCollider>();
			component.direction = 2;
			component.radius = CollisionState.CapsuleColliderRadius;
			component.height = CollisionState.CapsuleColliderHeight;
			component.isTrigger = true;
			return component;
		}

		private void onDrift(CarEventState e)
		{
			if (e == CarEventState.EVENT_BEGIN)
			{
				WaitSecondSmallGasCallback(isWaiting: false);
			}
			if (e == CarEventState.EVENT_GAS && carState.N2State.Level == 0)
			{
				absView.StartCoroutine(waitForFirstSmallGas());
			}
		}

		private void SetupGears()
		{
		}

		internal void fixedUpdate()
		{
			if (base.Active)
			{
				calculateRelativeVelocity();
				setControlState();
				applyBalance();
				addExtraGravity();
				applyGroundFriction();
				applyAirDrag();
				CalculateEnginePower();
				applyThrottle();
				applySteering();
				applyDragForSteer();
				applyAllForces();
				ApplyAllAngle();
				clearRemain();
				updateDropDownEffect();
				updateDriftState();
				updateAirGroundState();
			}
		}

		internal void calculateRelativeVelocity()
		{
			carState.velocity = rigidbody.velocity;
			relativeVelocity = transform.InverseTransformDirection(carState.velocity);
			LinearVelocity = relativeVelocity.magnitude;
			carState.LinearVelocity = LinearVelocity;
			totalForce.Set(0f, 0f, 0f);
			carState.relativeVelocity = relativeVelocity;
			carState.SpeedRatio = carState.relativeVelocity.z / (float)carModel.MaxSpeeds[0];
		}

		private void setControlState()
		{
			setWheelState(carState);
			if (groundState == 15 && carState.LinearVelocity > carModel.MaxLinearVelocity)
			{
				carModel.MaxLinearVelocity = carState.LinearVelocity;
			}
			carState.CallBacks.OnSetState(carState);
		}

		internal void setWheelState(CarState state)
		{
			if (state == null || state.view == null || state.Wheels == null)
			{
				return;
			}
			if (state.WheelHits == null)
			{
				state.WheelHits = new WheelHit[state.Wheels.Length];
			}
			wheels = state.Wheels;
			wheelHits = state.WheelHits;
			groundState = 0;
			tmp = this.transform.forward;
			int num = 0;
			for (int i = 0; i < wheels.Length; i++)
			{
				if (wheels[i] == null || wheels[i].collider == null)
				{
					continue;
				}
				bool isGrounded = wheels[i].collider.isGrounded;
				Transform transform = wheels[i].collider.transform;
				float num2 = (wheels[i].driveWheel ? carModel.RearWheelRadius : carModel.FrontWheelRadius);
				if (wheels[i].collider.GetGroundHit(out wheelHits[i]))
				{
					num |= 1 << i;
					carState.HitLenghts[i] = Mathf.Lerp(carState.HitLenghts[i], Vector3.Distance(wheelHits[i].point, wheels[i].collider.bounds.center - carState.rigidbody.velocity * Time.deltaTime), 0.1f);
				}
				else
				{
					carState.HitLenghts[i] = 0f;
				}
				if (isGrounded)
				{
					groundState |= 1 << i;
					if (Application.isEditor)
					{
						Debug.DrawLine(wheelHits[i].point, carState.rigidbody.position - carState.rigidbody.velocity * Time.deltaTime, Color.white);
					}
				}
			}
			canDrive = (groundState & 0xC) != 0;
			canSteer = (groundState & 3) != 0;
			state.GroundHit = groundState;
			state.WheelHitState = num;
			carState.ThrottleDirection = Vector3.Project(this.transform.forward, wheelHits[(groundState < 8) ? 2 : 3].forwardDir).normalized;
		}

		private void setCrashState(CarState state)
		{
			if (state.CollisionDirection != 0)
			{
				curHitDot = Vector3.Dot(state.HitNormal, state.transform.forward);
			}
			if (noXDrag && Time.time - noXTime >= 0.4f)
			{
				noXDrag = false;
			}
			if (lockRotY && (state.view.RunState == RunState.Crash || Time.time - lockYTime >= 0.1f))
			{
				lockRotY = false;
				if (state.CollisionDirection != 0)
				{
					noXDrag = true;
					noXTime = Time.time;
				}
			}
			if (state.CollisionDirection == HitWallDirection.FRONT || state.CollisionDirection == HitWallDirection.BACK)
			{
				carState.HitTime = Time.time;
				if (Mathf.Abs(carState.HitDot) > 0.7f)
				{
					state.N2Force = false;
				}
				if (!(Mathf.Abs(curHitDot) > 0.5f) || !(carState.LinearVelocity < 2f))
				{
					return;
				}
				if (state.CollisionDirection == HitWallDirection.FRONT)
				{
					if (state.Throttle == 1f)
					{
						state.Throttle = 0f;
					}
				}
				else if (state.Throttle == -1f)
				{
					state.Throttle = 0f;
				}
			}
			else if (carState.GroundHit == 0 && state.CollisionDirection == HitWallDirection.NONE)
			{
				rigidbody.angularDrag = 1.5f;
			}
			else
			{
				rigidbody.angularDrag = carModel.AngularDrag;
			}
		}

		private void OnSmallGasBreak(CommonGasItem gas)
		{
			if (gas.Level == 2)
			{
				OnFirstSmallGasBreak();
			}
			gasItem = null;
		}

		private void OnFirstSmallGasBreak()
		{
			if (carState.view.Controller.drifter.PassAngle > (float)DriftController.SECOND_SMALL_GAS_ANGLE)
			{
				absView.StartCoroutine(waitForSecondSmallGas());
			}
		}

		internal void startSmallN2Force(CommonGasItem gas = null, bool isDriftTrigger = false)
		{
			if (carState.N2State.Level != 0)
			{
				return;
			}
			ItemParams itemParams = new ItemParams(null, null, 0);
			itemParams.user = carState;
			itemParams.targets = new CarState[1] { carState };
			if (gas == null)
			{
				gas = new CommonGasItem(2, carModel.SmallN2ForceTime);
			}
			gasItem = gas;
			if (gasItem.Usable(itemParams))
			{
				gasItem.BreakCallback = OnSmallGasBreak;
				gasItem.Toggle(itemParams);
				if (isDriftTrigger && carState.view.Controller.drifter.PassAngle > (float)DriftController.SECOND_SMALL_GAS_ANGLE)
				{
					WaitSecondSmallGasCallback(isWaiting: true);
				}
			}
			else
			{
				gasItem.Break();
				gasItem = null;
			}
		}

		private void StopSmallGas()
		{
			if (gasItem != null)
			{
				gasItem.Break();
			}
		}

		private void addExtraGravity()
		{
			if (carState.GroundHit != 0)
			{
				tmp = (0f - (carModel.ExtraGravity + (float)carModel.Gravity + 5f)) * (float)carModel.CarWeight * carState.GroundNormal;
			}
			else
			{
				tmp = (0f - (carModel.ExtraGravity + (float)carModel.Gravity + 8f)) * (float)carModel.CarWeight * carState.LastNormal;
			}
			carState.Gravity = tmp;
		}

		private void applyGroundFriction()
		{
			if (wheelHits == null || wheelHits.Length == 0)
			{
				return;
			}
			float num = 0f;
			int num2 = wheelHits.Length;
			int num3 = 0;
			for (int i = 0; i < num2; i++)
			{
				if (wheels[i].collider.isGrounded)
				{
					WheelHit wheelHit = wheelHits[i];
					Collider collider = wheelHit.collider;
					if ((bool)collider)
					{
						num3++;
						num += collider.material.dynamicFriction;
					}
				}
			}
			if (num3 != 0 && LinearVelocity > (float)SLEEP_LINEAR_VELOCITY)
			{
				num /= (float)num3;
				carState.GroundFriction = transform.rotation * CalculateGroundFrictionByLevel(carModel, num, -relativeVelocity.normalized, carState.N2State.Level) * RaceConfig.TimeFactor;
				if (carState.CollisionDirection != 0)
				{
					carState.GroundFriction.y = 0f;
				}
			}
		}

		private void updateFriction()
		{
			float num = relativeVelocity.x * relativeVelocity.x;
			wfc.extremumSlip = 1f;
			wfc.asymptoteSlip = 2f;
			wfc.stiffness = 1f;
			wfc.extremumValue = Mathf.Clamp(300f - num, 0f, 300f);
			wfc.asymptoteValue = Mathf.Clamp(150f - num / 2f, 0f, 150f);
			for (int i = 0; i < wheels.Length; i++)
			{
				Wheel wheel = wheels[i];
				wheel.collider.sidewaysFriction = wfc;
			}
		}

		private void applyAirDrag()
		{
			if (carState != null && !(LinearVelocity < (float)SLEEP_LINEAR_VELOCITY))
			{
				tmp = carState.AirDragMultiplier;
				tmp.x = 0f;
				Vector3 vector = CalculateAirDrag(tmp, carModel.AirFrictionFactor, relativeVelocity);
				vector.x *= (float)carModel.MaxSpeeds[0] / LinearVelocity;
				if (noXDrag)
				{
					float num = Mathf.Clamp01((Time.time - noXTime) / 0.4f);
					vector.x *= Mathf.Clamp01(num * num + 0.075f);
				}
				carState.AirDrag = transform.rotation * vector * RaceConfig.TimeFactor;
				Vector3 vector2 = vector;
				Debug.Log(" drag4:" + vector2.ToString());
				if (carState.CollisionDirection != 0)
				{
					carState.AirDrag.y = 0f;
				}
			}
		}

		public void CalculateEnginePower()
		{
		}

		private void applyThrottle()
		{
			if (wheelHits == null || !canDrive || carState == null)
			{
				return;
			}
			float throttle = carState.Throttle;
			if (((bool)wheelHits[2].collider && wheelHits[2].collider.gameObject.layer == 8) || ((bool)wheelHits[3].collider && wheelHits[3].collider.gameObject.layer == 8))
			{
				carState.ThrottleForce = (float)RaceConfig.TimeFactor * throttle * CalculateEngineForce(carModel, carState.SpeedRatio, 0) * carState.ThrottleDirection;
				if (carState.Throttle > 0f && carState.relativeVelocity.z < 0f)
				{
					carState.ThrottleForce *= (float)REFORWARD_FACTOR;
				}
				applyN2DownForces();
			}
		}

		internal void applyBalance()
		{
			calGroundNormal();
			if (carState != null && !(carState.ThrottleDirection == vzero) && carState.GroundHit != 0 && !(Time.time - carState.HitTime > 0.5f) && (carState.GroundHit == 5 || carState.GroundHit == 10) && forceUpward && carState.ApplyingSpecialType != SpecialType.Translate)
			{
				carState.rigidbody.MoveRotation(Quaternion.LookRotation(carState.rigidbody.rotation * vforward, carState.GroundNormal));
			}
		}

		internal void applyDragForSteer()
		{
			if (carState.Steer != 0f && carState.CurDriftState.Stage == DriftStage.NONE && carState.SpeedRatio > (float)carModel.MinRatioSteerDragOn)
			{
				float num = carModel.SteerDragCurve.Evaluate(Time.time - carState.SteerChangeTime);
				totalForce -= num * carState.ThrottleForce;
			}
		}

		internal void calGroundNormal()
		{
			if (carState.GroundHit == 0)
			{
				return;
			}
			int num = 0;
			tmp.Set(0f, 0f, 0f);
			for (int i = 0; i < carState.WheelHits.Length; i++)
			{
				WheelHit wheelHit = carState.WheelHits[i];
				if ((carState.GroundHit & (1 << i)) != 0)
				{
					tmp += wheelHit.normal;
					num++;
				}
			}
			if (num != 0)
			{
				tmp /= (float)num;
				carState.GroundNormal = tmp;
			}
		}

		private void updateDropDownEffect()
		{
			if (carState.GroundHit != 0 && lastGroundState == 0)
			{
				carState.view.WaitForDropDown();
				if (CameraController.Current != null && CameraController.Current.ViewTarget != null && CameraController.Current.ViewTarget == carState)
				{
					CameraController.Current.ShakeCamera(CameraController.ShakeType.YBack, 0.1f);
				}
			}
			lastGroundState = carState.GroundHit;
		}

		private void updateDriftState()
		{
			if (drifter == null)
			{
				return;
			}
			if (carState.GroundHit == 0 || lastGroundState == 0)
			{
				if (drifter.Active)
				{
					drifter.Active = false;
				}
			}
			else if (drifter.Active != base.Active)
			{
				drifter.Active = base.Active;
			}
		}

		private void updateAirGroundState()
		{
			if (carState.GroundHit == 0 && carState.ApplyingSpecialType != SpecialType.Translate && lastGroundState == 0 && groundState == 0)
			{
				carState.AirGround.UpdateAirGroundState(onGround: false);
			}
			if (carState.GroundHit != 0 && carState.ApplyingSpecialType != SpecialType.Translate && lastGroundState != 0 && groundState != 0)
			{
				carState.AirGround.UpdateAirGroundState(onGround: true);
			}
		}

		private void applySteering()
		{
			if (carState == null || !canSteer)
			{
				return;
			}
			HitWallDirection collisionDirection = carState.CollisionDirection;
			float num = 1f;
			if (carState.CurDriftState.Stage == DriftStage.NONE && RaceConfig.SteerTimeScale)
			{
				num = Mathf.Clamp01((float)carModel.MinRoateRotio + (Time.time - carState.SteerChangeTime) / (float)carModel.FullRotateTime);
			}
			float num2 = carState.CallBacks.OnSteer(carState) * num;
			float num3 = 0f;
			float num4 = 1f;
			carState.view.CenterOfMassController.ApplyCenterOfMassOffset(num2, carState);
			if (LinearVelocity < (float)SLEEP_LINEAR_VELOCITY && carState.Throttle == 0f)
			{
				num4 = 1f;
			}
			else
			{
				num4 = Mathf.Sign(relativeVelocity.z);
			}
			bool flag = false;
			if (collisionDirection != 0 && num2 != 0f && carState.LinearVelocity < 2f)
			{
				tmpVector = rigidbody.angularVelocity;
				if (tmpVector.sqrMagnitude < (float)HIT_WALL_EXTRA_STEER * (float)HIT_WALL_EXTRA_STEER)
				{
					flag = true;
					carState.rigidbody.rotation *= ((num2 < 0f) ? leftExtra : rightExtra);
				}
				exSteerTime = Time.time;
			}
			if (!flag)
			{
				num3 = CalculateTorque(1f);
				if (carState.Wheels != null && carState.Wheels.Length > 1)
				{
					num3 *= num2 * 0.1f;
					carState.Wheels[0].collider.steerAngle = num3;
					carState.Wheels[1].collider.steerAngle = num3;
				}
			}
		}

		private IEnumerator waitForFirstSmallGas()
		{
			_ = Time.realtimeSinceStartup;
			while (carState.Steer != 0f || carState.CurDriftState.Stage == DriftStage.SLIDING || carState.CurDriftState.Stage == DriftStage.ROTATING || carState.CurDriftState.Stage == DriftStage.CIRCLING)
			{
				yield return 1;
			}
			if (gasItem == null)
			{
				startSmallN2Force(null, isDriftTrigger: true);
			}
		}

		public bool TryStartSecondSmallGas()
		{
			if (carState.CurDriftState.Stage == DriftStage.NONE && carState.GroundHit != 0 && carState.N2State.Level == 0 && _waitingForSecondSmallGas && gasItem == null)
			{
				startSmallN2Force(new CommonGasItem(3, carModel.SmallN2ForceTime2));
				_waitingForSecondSmallGas = false;
				WaitSecondSmallGasCallback(isWaiting: false);
				return true;
			}
			return false;
		}

		private void WaitSecondSmallGasCallback(bool isWaiting)
		{
			if (carState.CallBacks.OnWaitSecondSmallGas != null)
			{
				carState.CallBacks.OnWaitSecondSmallGas(isWaiting);
			}
		}

		private IEnumerator waitForSecondSmallGas()
		{
			float startTime = Time.realtimeSinceStartup;
			_waitingForSecondSmallGas = true;
			while (Time.realtimeSinceStartup - startTime <= (float)carModel.SECOND_SMALL_GAS_WAIT_TIME)
			{
				yield return 1;
			}
			_waitingForSecondSmallGas = false;
			WaitSecondSmallGasCallback(isWaiting: false);
		}

		private void applyN2DownForces()
		{
			if (carState.N2State.Level == 0 && carState.SpeedRatio > 1f)
			{
				Vector3 vector = carState.ThrottleDirection * -1f * ((carState.N2State.PreLevel == 1) ? carModel.N2DownFactor : carModel.SmallN2DownFactor);
				carState.ThrottleForce += vector;
			}
		}

		private void applyAllForces()
		{
			if (groundState > 0 && totalForce.y > 0f)
			{
				totalForce.y = 0f;
			}
			totalForce += carState.AirDrag + carState.GroundFriction + carState.ThrottleForce + carState.Gravity;
			carState.TotalForces = totalForce;
			carState.CallBacks.OnApplyForce();
			if (!float.IsNaN(totalForce.x) && !float.IsNaN(totalForce.y) && !float.IsNaN(totalForce.z))
			{
				Debug.LogWarning(" --- Modify by Sbin in CarController: applyAllForces()   The motor torque is used to drive the wheel Collider, because the car cannot start when only rigidbody.addforce is used ");
				Wheel[] array = wheels;
				foreach (Wheel wheel in array)
				{
					if (wheel.driveWheel)
					{
						if (carState.Throttle != 0f && carState.velocity.sqrMagnitude <= 0.001f)
						{
							wheel.collider.motorTorque = carState.Throttle * Mathf.Abs(carState.ThrottleForce.z);
						}
						else if (carState.velocity.sqrMagnitude > 0.001f)
						{
							wheel.collider.motorTorque = 0f;
						}
					}
				}
				rigidbody.AddForce(carState.TotalForces);
				Debug.DrawRay(transform.position, carState.TotalForces, Color.red);
			}
			else
			{
				Log("NaN Force carState.AirDrag: ", carState.AirDrag, " carState.GroundFriction:", carState.GroundFriction, " carState.ThrottleForce:", carState.ThrottleForce, " carState.HitNormal:", carState.HitNormal);
			}
		}

		private void ApplyAllAngle()
		{
			DoRotAngle(ref totalAngle, angleSpeed);
			float rotAngle = carState.CollisionState.RotAngle;
			DoRotAngle(ref rotAngle, CollisionState.AngleSpeed);
			carState.CollisionState.RotAngle = rotAngle;
		}

		private void DoRotAngle(ref float rotAngle, float speed)
		{
			float num = Mathf.Abs(rotAngle);
			if (num > 0f)
			{
				float num2 = rotAngle / num;
				float a = Time.deltaTime * speed;
				a = Mathf.Min(a, num);
				float num3 = num2 * a;
				rigidbody.rotation *= Quaternion.Euler(0f, num3, 0f);
				rotAngle -= num3;
			}
		}

		public void clearRemain()
		{
			if (carState.Throttle == 0f && LinearVelocity < (float)SLEEP_LINEAR_VELOCITY && !rigidbody.isKinematic)
			{
				tmpVector = rigidbody.velocity;
				tmpVector.x = 0f;
				tmpVector.z = 0f;
				rigidbody.velocity = tmpVector;
			}
		}

		private void onTriggerSpecialBegin(Collider c)
		{
			switch (c.gameObject.layer)
			{
				case 20:
					onToggleObstacle(c);
					break;
				case 18:
					onToggleSpeedUp(c);
					break;
				case 28:
					onToggleSustainedSpeedUp(c);
					break;
				case 19:
					onToggleTranslation(c);
					break;
			}
		}

		private void onTriggerSpecialEnd(Collider c)
		{
			int layer = c.gameObject.layer;
			if (layer == 28)
			{
				onToggleSustainedSpeedUpBreak(c);
			}
		}

		private void onTriggerSpecialStaying(Collider c)
		{
			if (c.gameObject.layer == 28 && Time.time - _lastSpecialStayingTime >= _specialStayingInterval)
			{
				onToggleSustainedSpeedUp(c);
				_lastSpecialStayingTime = Time.time;
			}
		}

		private void onToggleSpeedUp(Collider c)
		{
			if (!(Time.time - lastSpTime < 1.2f) || !(c == lastSpeedTrigger))
			{
				lastSpTime = Time.time;
				lastSpeedTrigger = c;
				SpeedUpTrigger speedUpTrigger = new SpeedUpTrigger();
				speedUpTrigger.Toggle(carState);
			}
		}

		private void onToggleSustainedSpeedUp(Collider c)
		{
			if (lastSustainedSpeedUp == null)
			{
				lastSustainedSpeedUp = new SustainedSpeedUpTrigger();
			}
			SustainedSpeedUpTrigger sustainedSpeedUpTrigger = (SustainedSpeedUpTrigger)lastSustainedSpeedUp;
			if (carState.Throttle < 0f)
			{
				lastSustainedSpeedUp.Break();
			}
			else if (carState.N2State.Level == 0 && !sustainedSpeedUpTrigger.IsRunning)
			{
				if (gasItem != null)
				{
					gasItem.Break();
					gasItem = null;
				}
				_smallGasQue.Clear();
				lastSustainedSpeedUp.Toggle(carState);
			}
		}

		private void onToggleSustainedSpeedUpBreak(Collider c)
		{
			if (lastSustainedSpeedUp != null)
			{
				lastSustainedSpeedUp.Break();
			}
		}

		private void onToggleTranslation(Collider c)
		{
			if (carState.ApplyingSpecialType == SpecialType.Translate || c.tag == "Forbidden")
			{
				return;
			}
			TranslateStarter component = c.gameObject.GetComponent<TranslateStarter>();
			if (component == null)
			{
				return;
			}
			TranslatePath path = null;
			carState.rigidbody = rigidbody;
			int pathCloest = component.GetPathCloest(carState, ref path);
			TranslateTrigger translateTrigger = new TranslateTrigger(path, path.TotalLength / (float)RaceConfig.TranslateSpeed * component.TimeScale, component.EndTrigger, component.PushGrounded, component.TimeCurve);
			base.Active = false;
			translateTrigger.AddCallback(delegate(SpecialTriggerBase s, SpecialCallback sc)
			{
				if (sc == SpecialCallback.Toggle && RaceAudioManager.ActiveInstance != null)
				{
					carState.view.ExEffectSource.clip = RaceAudioManager.ActiveInstance.Sound_translate;
					carState.view.ExEffectSource.loop = true;
					carState.view.ExEffectSource.Play();
				}
				if (sc == SpecialCallback.Stop)
				{
					base.Active = true;
					carState.view.ExEffectSource.Stop();
					carState.view.ExEffectSource.clip = null;
					carState.view.ExEffectSource.loop = false;
				}
			});
			carState.TranslateInfoByte = (byte)((uint)(component.Id << 3) | ((uint)pathCloest & 7u));
			translateTrigger.Toggle(carState);
		}

		private void onToggleObstacle(Collider c)
		{
		}

		public bool StartToWait()
		{
			if (carState.CollisionDirection != HitWallDirection.FRONT)
			{
				return carState.CollisionDirection == HitWallDirection.ABOVE;
			}
			return true;
		}

		public bool Cancelable(object data)
		{
			return false;
		}

		public bool NeedReset(object userData)
		{
			bool r = true;
			if (carState.CallBacks.ResetChecker != null)
			{
				carState.CallBacks.ResetChecker(ref r);
			}
			if (!r)
			{
				return false;
			}
			if (LinearVelocity < MaxResetSpeed)
			{
				tmp = carState.transform.position;
				if (userData != null)
				{
					tmp = (Vector3)userData;
				}
				float sqrMagnitude = (carState.transform.position - tmp).sqrMagnitude;
				if (sqrMagnitude < MaxResetDistance * MaxResetDistance)
				{
					return true;
				}
			}
			return false;
		}

		public void OnReset()
		{
			RacePathNode curNode = carState.CurNode;
			if (curNode == null)
			{
				LogWarning("No path node reached.");
				return;
			}
			Transform transform = ((curNode.LeftNode == null) ? curNode.transform : curNode.LeftNode.transform);
			Transform transform2 = carState.transform;
			tmp = curNode.transform.position;
			Vector3 roadCenter = GetRoadCenter(carState, out var _);
			roadCenter = ((!Physics.Raycast(roadCenter + carState.LastNormal, -carState.LastNormal, out var hitInfo, 50f, 256)) ? tmp : hitInfo.point);
			roadCenter += RaceConfig.ResetOffsetY * carState.LastNormal;
			transform2.position = roadCenter;
			transform2.rotation = Quaternion.LookRotation(Vector3.Cross(Vector3.Cross(carState.LastNormal, curNode.Forward), carState.LastNormal), carState.LastNormal);
			if (carState.view.DriftEnable)
			{
				carState.view.DriftEnable = false;
				carState.view.DriftEnable = true;
			}
		}

		internal static float CalculateTopSpeed(CarModel param, int level = 0)
		{
			float z = CalculateGroundFrictionByLevel(param, 0.4f, Vector3.forward, level).z;
			float num = 0f;
			for (int i = 0; i <= level; i++)
			{
				num += CalculateEngineForce(param, 1f, i);
			}
			return Mathf.Sqrt(Mathf.Max(0f, (num - z) / (param.DragMultiplier.z * (float)param.AirFrictionFactor)));
		}

		internal static Vector3 CalculateGroundFriction(CarModel param, float friction, Vector3 direction)
		{
			return friction * (float)param.CarWeight * (float)param.GroundFrictionFactor * direction;
		}

		internal static Vector3 CalculateGroundFrictionByLevel(CarModel param, float friction, Vector3 direction, int level)
		{
			if (1 == level)
			{
				return CalculateGroundFriction(param, friction, direction);
			}
			return param.GroundFrictionScale * CalculateGroundFriction(param, friction, direction);
		}

		internal static Vector3 CalculateAirDrag(Vector3 multiplier, float dragFactor, Vector3 relativeVelocity)
		{
			Vector3 b = new Vector3((0f - relativeVelocity.x) * Mathf.Abs(relativeVelocity.x), (0f - relativeVelocity.y) * Mathf.Abs(relativeVelocity.y), (0f - relativeVelocity.z) * Mathf.Abs(relativeVelocity.z));
			return Vector3.Scale(multiplier, b) * dragFactor;
		}

		internal static float CalculateEngineForce(CarModel param, float ratio, int i)
		{
			return 100f * ((float)param.EngineForces[i] * param.EngineCurves[i].Evaluate(Mathf.Abs(ratio)) + (float)param.DeltaAcceleration);
		}

		internal static float CalculateEngineForce(float engineForce, AnimationCurve engineCurve, float ratio)
		{
			return 100f * (engineForce * engineCurve.Evaluate(Mathf.Abs(ratio)));
		}

		internal float CalculateTorque(float sign)
		{
			return sign * ((float)carModel.MinimumTurn + carModel.SteerCurve.Evaluate(LinearVelocity / 80f) * ((float)carModel.MaximumTurn - (float)carModel.MinimumTurn));
		}

		private bool HaveTheSameSign(float first, float second)
		{
			if (Mathf.Sign(first) == Mathf.Sign(second))
			{
				return true;
			}
			return false;
		}

		private float EvaluateNormPower(float normPower)
		{
			if (normPower < 1f)
			{
				return 10f - normPower * 9f;
			}
			return 1.9f - normPower * 0.9f;
		}

		private float EvaluateSpeedToTurn(float speed)
		{
			if (speed > (float)carModel.MaxSpeeds[0] * 0.5f)
			{
				return carModel.MinimumTurn;
			}
			float num = 1f - speed / ((float)carModel.MaxSpeeds[0] * 0.5f);
			return (float)carModel.MinimumTurn + num * ((float)carModel.MaximumTurn - (float)carModel.MinimumTurn);
		}

		private float Convert_Kms_Per_Hour_To_Meters_Per_Second(float value)
		{
			return value / 3.6f;
		}

		public static Vector3 GetRoadCenter(CarState carState, out Vector3 forward)
		{
			if (carState == null)
			{
				forward = new Vector3(0f, 0f, 0f);
				return new Vector3(0f, 0f, 0f);
			}
			Vector3 vector = carState.HitNormal;
			vector.y = 0f;
			Vector3 normalized = (carState.transform.position - carState.HitPoint).normalized;
			normalized.y = 0f;
			if (Vector3.Dot(vector, normalized) < 0f)
			{
				vector = -vector;
			}
			vector.y = 0.1f;
			if (Physics.Raycast(carState.HitPoint, vector, out var hitInfo, 5f, 512))
			{
				forward = vector;
				forward.y = 0f;
				forward = eu90y * forward;
				return (carState.HitPoint + hitInfo.point) * 0.5f;
			}
			forward = vector;
			forward.y = 0f;
			forward = eu90y * forward;
			return carState.CurNode.transform.position + vector * 2f;
		}
	}
}
