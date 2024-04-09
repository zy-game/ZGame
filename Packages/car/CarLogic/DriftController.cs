using System;
using UnityEngine;

namespace CarLogic
{
	public class DriftController : ControllerBase
	{
		private CarState carState;

		private CarModel carModel;

		private Transform transform;

		private Rigidbody rigidbody;

		private Vector3 totalForce;

		private Vector3 vt1;

		private Vector3 vt2;

		private Vector3 vt3;

		private float radius = 4f;

		private Vector3 forward = Vector3.forward;

		private bool praised;

		private bool broken;

		private bool _triggedGas;

		private bool _triggedDragDrift;

		private Vector3 _driftCarPreForward = Vector3.forward;

		private float _passAngle;

		private Vector3 _driftCentripetalForceCenter = Vector3.zero;

		private readonly Quaternion left = Quaternion.Euler(0f, -90f, 0f);

		private readonly Quaternion right = Quaternion.Euler(0f, 90f, 0f);

		public static int ROTATE_ACCEL_TYPE = 2;

		public static Float ROTATE_ACCEL_DEVIDE = 0.75f;

		public static Float GAS_TRIGGER_SPEED = 1.8f;

		public static Float REMAIN_STOP_SPEED = 0.06f;

		public static Float CIRCLE_FIXTIME = 0.3f;

		public static Float ROTATE_TO_CIRCLE_SPEED = 4;

		public static Float CIRCLE_TO_ROTATE_SPEED = 16;

		public static Float SLIDING_AIRDRAG_FACTOR = 0.99f;

		public static Float ROTATE_AIRDRAG_FACTOR = 0.9f;

		public static Float ROTATE_GF_FACTOR = 1;

		private static Keyframe[] SLIDING_GF_FACTOR_CURVE_KEYFRAMES = new Keyframe[5]
		{
			new Keyframe(0f, -2.5f),
			new Keyframe(0.6f, -3.5f),
			new Keyframe(1f, -3f),
			new Keyframe(2f, 0f),
			new Keyframe(2.8f, 1f)
		};

		public static AnimationCurve SLIDING_GF_FACTOR_CURVE = new AnimationCurve(SLIDING_GF_FACTOR_CURVE_KEYFRAMES);

		private static Keyframe[] MIN_DRIFT_RADIUS_KEYFRAMES = new Keyframe[2]
		{
			new Keyframe(0f, 1f),
			new Keyframe(1f, 1f)
		};

		public static AnimationCurve MIN_DRIFT_RADIUS_CURVE = new AnimationCurve(MIN_DRIFT_RADIUS_KEYFRAMES);

		public static Float CENTRIFUGAL_FORCE = 180;

		public static Float SPEED_DOWN_FORCE_FACTOR = 1;

		private static Keyframe[] DRIFT_SPEED_DOWN_KEYFRAMES = new Keyframe[9]
		{
			new Keyframe(0f, 0f),
			new Keyframe(0.3f, 0f),
			new Keyframe(0.4f, 1f),
			new Keyframe(0.5f, 5f),
			new Keyframe(0.6f, 5f),
			new Keyframe(0.7f, 10f),
			new Keyframe(0.78f, 15f),
			new Keyframe(0.8f, 50f),
			new Keyframe(1f, 100f)
		};

		public static AnimationCurve DRIFT_SPEED_DWON_CURVE = new AnimationCurve(DRIFT_SPEED_DOWN_KEYFRAMES);

		public static float DRIFT_DOWN_SPEED_MAX = 30f;

		public static Float ROTATE_SIDEWAY_FRICRION = 0.02f;

		public static Float REMAIN_AIRDRAG_FACTOR = 0.99f;

		public static Float REMAIN_THROTTLE_FACTOR = 0;

		public static Float SECOND_SMALL_GAS_ANGLE = 40;

		public static Float MIN_DRIFT_SPEED = 15;

		public static Float DRIFT_ENGINE_FROCE_FACTOR = 1;

		public static Float DRAG_FORCE = 0;

		public static Float DRIFT_ROTATING_ROTATE_FACTOR = 2.2f;

		public static Float DRIFT_SLIDING_ROTATE_FACTOR = 1;

		private static Keyframe[] DRIFT_FRONT_SIDEWAY_FACTOR_KEYFRAMES = new Keyframe[3]
		{
			new Keyframe(0f, 1f),
			new Keyframe(30f, 1f),
			new Keyframe(60f, 0.9f)
		};

		public static AnimationCurve DRIFT_FRONT_SIDEWAY_FACTOR_CURVE = new AnimationCurve(DRIFT_FRONT_SIDEWAY_FACTOR_KEYFRAMES);

		public static float DRIFT_AIR_Y_VELOCIT = -1.1f;

		private float a1;

		private float a2;

		private float t1;

		private float t2;

		private float r0;

		public float PassAngle => _passAngle;

		public void Init(CarState state)
		{
			if (state != null)
			{
				carState = state;
				carModel = state.view.carModel;
				transform = state.transform;
				rigidbody = state.rigidbody;
				if (ROTATE_ACCEL_TYPE == 2)
				{
					float num = ROTATE_ACCEL_DEVIDE;
					t1 = (float)carModel.Rotate0To90Time * num;
					t2 = (float)carModel.Rotate0To90Time - t1;
					a1 = (float)Math.PI / (t1 * t1 + num * t2 * t2 / (1f - num));
					a2 = a1 * num / (1f - num);
				}
				else if (ROTATE_ACCEL_TYPE == 3)
				{
					t1 = (float)carModel.Rotate0To90Time * (float)ROTATE_ACCEL_DEVIDE;
					t2 = (float)carModel.Rotate0To90Time - t1;
					a1 = (float)Math.PI / (t1 * t1 + 2f * t1 * t2);
				}
				else if (ROTATE_ACCEL_TYPE == 4)
				{
					r0 = Mathf.Clamp(1f, 0f, (float)Math.PI / 2f / (float)carModel.Rotate0To90Time);
					a1 = ((float)Math.PI - 2f * r0 * (float)carModel.Rotate0To90Time) / (float)carModel.Rotate0To90Time / (float)carModel.Rotate0To90Time;
				}
			}
		}

		public override void OnActiveChange(bool active)
		{
			if (carState != null && carState.view != null)
			{
				CarCallBack callBacks = carState.CallBacks;
				callBacks.OnApplyForce = (Action)Delegate.Remove(callBacks.OnApplyForce, new Action(onFixedUpdate));
				if (active)
				{
					CarCallBack callBacks2 = carState.CallBacks;
					callBacks2.OnApplyForce = (Action)Delegate.Combine(callBacks2.OnApplyForce, new Action(onFixedUpdate));
				}
				else if (carState.CurDriftState.Stage != 0)
				{
					Stop();
				}
			}
		}

		private void UpdatePassAngle()
		{
			_passAngle += Vector3.Angle(_driftCarPreForward, carState.transform.forward);
			_driftCarPreForward = carState.transform.forward;
		}

		private void TriggerGas()
		{
			if (carState.CurDriftState.Stage == DriftStage.REMAIN && !_triggedGas && carState.CallBacks.OnDrift != null && Time.time - carState.CurDriftState.StartTime > (float)carModel.PerfectDriftTime)
			{
				UpdatePassAngle();
				carState.CallBacks.OnDrift(CarEventState.EVENT_GAS);
				_triggedGas = true;
			}
		}

		public void Stop()
		{
			DriftState curDriftState = carState.CurDriftState;
			if (curDriftState.Stage != 0)
			{
				if (curDriftState.Stage != DriftStage.REMAIN && RaceConfig.LeanCameraOnDrift && CameraController.Current != null && CameraController.Current.ViewTarget == carState)
				{
					CameraController.Current.StartLeanTo(0f, RaceConfig.CamLeanEndFactor);
				}
				if (!broken && _triggedGas)
				{
					curDriftState.GoodCount++;
					if (carState.CallBacks.OnGoodDrift != null)
					{
						carState.CallBacks.OnGoodDrift(carState);
					}
				}
				curDriftState.Stage = DriftStage.NONE;
				carState.view.EnableSidewayFriction();
				if (carState.CallBacks.OnDrift != null)
				{
					carState.CallBacks.OnDrift(CarEventState.EVENT_END);
				}
			}
			CarCallBack callBacks = carState.CallBacks;
			callBacks.InverseChecker = (ModifyAction<bool>)Delegate.Remove(callBacks.InverseChecker, new ModifyAction<bool>(checker));
			CarCallBack callBacks2 = carState.CallBacks;
			callBacks2.ResetChecker = (ModifyAction<bool>)Delegate.Remove(callBacks2.ResetChecker, new ModifyAction<bool>(checker));
		}

		private void checker(ref bool enable)
		{
			if (carState.CurDriftState.Stage == DriftStage.ROTATING)
			{
				enable = false;
			}
			else
			{
				enable = true;
			}
		}

		private void onFixedUpdate()
		{
			if (base.Active && carState != null)
			{
				setState();
				applyDrift();
				applyState();
			}
		}

		private void setState()
		{
			DriftState curDriftState = carState.CurDriftState;
			if (carState.GroundHit != 0)
			{
				curDriftState.LastGroundedTime = Time.time;
			}
			if (carState.view.RunState == RunState.Crash)
			{
				if (carState.CurDriftState.Stage != 0)
				{
					broken = true;
					carState.CurDriftState.GoodCount = 0;
					if (carState.CallBacks.OnDrift != null)
					{
						carState.CallBacks.OnDrift(CarEventState.EVENT_BREAK);
					}
				}
				Stop();
				carState.Drift = false;
			}
			if (curDriftState.Stage != 0 && carState.ApplyingSpecialType != SpecialType.Translate && Time.time - curDriftState.LastGroundedTime > 0.5f)
			{
				if (carState.CallBacks.OnDrift != null)
				{
					carState.CallBacks.OnDrift(CarEventState.EVENT_BREAK);
				}
				Stop();
				carState.Drift = false;
			}
			totalForce.Set(0f, 0f, 0f);
			curDriftState.veldot = Vector3.Dot(forward, carState.relativeVelocity.normalized);
			curDriftState.dirdot = Vector3.Dot(curDriftState.StartDirection, carState.velocity.normalized);
			if (curDriftState.Stage == DriftStage.CIRCLING || curDriftState.Stage == DriftStage.ROTATING)
			{
				praised = true;
			}
			else if (curDriftState.Stage == DriftStage.SLIDING && curDriftState.veldot < 0.705f)
			{
				praised = true;
			}
		}

		private void applyState()
		{
			carState.TotalForces += totalForce;
		}

		private void driftStart()
		{
			DriftState curDriftState = carState.CurDriftState;
			curDriftState.StartDirection = carState.ThrottleDirection;
			curDriftState.TimePast = 0f;
			curDriftState.StartLinearVelocity = Vector3.Project(rigidbody.velocity, curDriftState.StartDirection).sqrMagnitude;
			curDriftState.Stage = DriftStage.SLIDING;
			curDriftState.StartTime = Time.time;
			curDriftState.StartSteer = carState.CallBacks.OnSteer(carState);
			curDriftState.NoRemainFlag = 0;
			if (carState.CallBacks.OnDrift != null)
			{
				carState.CallBacks.OnDrift(CarEventState.EVENT_BEGIN);
			}
			CarCallBack callBacks = carState.CallBacks;
			callBacks.InverseChecker = (ModifyAction<bool>)Delegate.Remove(callBacks.InverseChecker, new ModifyAction<bool>(checker));
			CarCallBack callBacks2 = carState.CallBacks;
			callBacks2.InverseChecker = (ModifyAction<bool>)Delegate.Combine(callBacks2.InverseChecker, new ModifyAction<bool>(checker));
			CarCallBack callBacks3 = carState.CallBacks;
			callBacks3.ResetChecker = (ModifyAction<bool>)Delegate.Remove(callBacks3.ResetChecker, new ModifyAction<bool>(checker));
			CarCallBack callBacks4 = carState.CallBacks;
			callBacks4.ResetChecker = (ModifyAction<bool>)Delegate.Combine(callBacks4.ResetChecker, new ModifyAction<bool>(checker));
			carState.view.DisableSidewayFriction();
			if (RaceConfig.LeanCameraOnDrift && CameraController.Current != null && CameraController.Current.ViewTarget == carState)
			{
				CameraController.Current.StartLeanTo(curDriftState.StartSteer, RaceConfig.CamLeanStartFactor);
			}
			broken = false;
			praised = false;
			_triggedGas = false;
			_triggedDragDrift = false;
			_passAngle = 0f;
			_driftCarPreForward = carState.transform.forward;
			_driftCentripetalForceCenter = carState.transform.position + carState.transform.right * carModel.MinDriftRadius;
		}

		private void applyDriftSlidingFriction()
		{
			totalForce += SLIDING_GF_FACTOR_CURVE.Evaluate(carState.CurDriftState.TimePast) * carState.GroundFriction * carState.SpeedRatio;
		}

		private void applyDriftSpeedDownForce()
		{
			totalForce -= carState.transform.forward * DRIFT_SPEED_DWON_CURVE.Evaluate(carState.relativeVelocity.z / DRIFT_DOWN_SPEED_MAX) * SPEED_DOWN_FORCE_FACTOR;
		}

		private void applyCentripetalForce()
		{
			if (_passAngle >= 45f)
			{
				totalForce += (_driftCentripetalForceCenter - carState.transform.position) * Mathf.Abs(Mathf.Cos(_passAngle)) * CENTRIFUGAL_FORCE;
			}
		}

		private void onStage(DriftState dfState)
		{
			if (dfState.Stage == DriftStage.REMAIN)
			{
				if ((carState.Steer == 0f || Mathf.Abs(carState.relativeVelocity.x) < (float)REMAIN_STOP_SPEED) && carState.CallBacks.OnDragDriftStop != null && _triggedDragDrift)
				{
					carState.CallBacks.OnDragDriftStop(carState);
					_triggedDragDrift = false;
				}
				if (Mathf.Abs(carState.relativeVelocity.x) < (float)GAS_TRIGGER_SPEED)
				{
					TriggerGas();
				}
				if (Mathf.Abs(carState.relativeVelocity.x) < (float)REMAIN_STOP_SPEED)
				{
					Stop();
					return;
				}
			}
			if (carState.GroundHit == 0)
			{
				return;
			}
			if (dfState.Stage == DriftStage.ROTATING)
			{
				vt1 = carState.velocity;
				if (vt1.sqrMagnitude / (float)carModel.MaxSpeeds[0] / (float)carModel.MaxSpeeds[0] < calRotateToCircleRatio(carModel) * calRotateToCircleRatio(carModel))
				{
					dfState.Stage = DriftStage.CIRCLING;
					dfState.NoRemainFlag = 1;
				}
			}
			if (dfState.Stage == DriftStage.REMAIN)
			{
				float num = Mathf.Clamp01((Time.time - dfState.RemainStartAt) / dfState.FullRemainTime);
				carState.view.SetSideWayFriction(carModel.SideWayFriction * num * num);
				carState.view.SetFrontSideWayFriction(carModel.SideWayFriction * num * num * DRIFT_FRONT_SIDEWAY_FACTOR_CURVE.Evaluate(_passAngle));
				vt1 = carState.relativeVelocity;
				vt1.Set(0f, 0f, 0f - Mathf.Min(0f, vt1.z));
				totalForce += carState.rigidbody.rotation * (vt1 * carState.rigidbody.mass * 10f);
				return;
			}
			if (dfState.Stage == DriftStage.SLIDING)
			{
				totalForce -= carState.ThrottleForce + SLIDING_AIRDRAG_FACTOR * carState.AirDrag + carState.Gravity;
				applyDriftSlidingFriction();
				carState.view.SetSideWayFriction((1f - (float)SLIDING_AIRDRAG_FACTOR) * carModel.SideWayFriction);
				applyDriftSpeedDownForce();
				applyCentripetalForce();
			}
			else if (dfState.Stage == DriftStage.CIRCLING)
			{
				totalForce -= carState.ThrottleForce + carState.AirDrag + carState.GroundFriction + carState.Gravity;
				carState.view.SetSideWayFriction(0f);
				applyDriftSpeedDownForce();
				applyCentripetalForce();
			}
			else
			{
				carState.view.SetSideWayFriction((float)ROTATE_SIDEWAY_FRICRION * carModel.SideWayFriction);
				totalForce -= carState.ThrottleForce + ROTATE_AIRDRAG_FACTOR * carState.AirDrag + ROTATE_GF_FACTOR * carState.GroundFriction + carState.Gravity;
				applyDriftSpeedDownForce();
				applyCentripetalForce();
			}
			float num2 = 1f;
			float num3 = Mathf.Clamp01((Time.time - dfState.StartTime) / (float)carModel.Rotate0To90Time);
			float num4 = 0f;
			vt1 = carState.velocity;
			float linearVelocity = carState.LinearVelocity;
			radius = carModel.MinDriftRadius;
			float b = MIN_DRIFT_RADIUS_CURVE.Evaluate(carState.relativeVelocity.z / (float)carState.view.carModel.MaxSpeeds[0]);
			radius *= Mathf.Max(1E-06f, b);
			switch (dfState.Stage)
			{
				case DriftStage.SLIDING:
					if (Time.time - dfState.StartTime > (float)carModel.Rotate0To90Time)
					{
						dfState.Stage = DriftStage.ROTATING;
					}
					else
					{
						num2 = num3 * num3;
					}
					break;
				case DriftStage.CIRCLING:
				{
					if (vt1.sqrMagnitude / (float)carModel.MaxSpeeds[0] / (float)carModel.MaxSpeeds[0] > calCircleToRotateRatio(carModel) * calCircleToRotateRatio(carModel))
					{
						dfState.Stage = DriftStage.ROTATING;
						break;
					}
					float num5 = (calCircleToRotateRatio(carModel) - calRotateToCircleRatio(carModel)) * (float)carModel.MaxSpeeds[0] / (float)CIRCLE_FIXTIME;
					totalForce += rigidbody.mass * num5 * vt1.normalized;
					break;
				}
			}
			num4 = rigidbody.mass * linearVelocity * linearVelocity / radius * num2;
			vt3 = carState.GroundNormal;
			if (vt3.sqrMagnitude < 0.25f)
			{
				vt3 = carState.LastNormal;
			}
			if (dfState.StartSteer < 0f)
			{
				vt2 = Vector3.Cross(vt1, vt3);
			}
			else
			{
				vt2 = Vector3.Cross(vt3, vt1);
			}
			vt2 = vt2.normalized * num4;
			totalForce += vt2;
			totalForce -= carState.rigidbody.mass * 30f * vt3;
			vt3 = (vt1 + Time.deltaTime / rigidbody.mass * vt2).normalized;
			vt2 = Vector3.Project(vt3 * linearVelocity, vt1) - vt1;
			vt2 *= rigidbody.mass / Time.deltaTime;
			totalForce += vt2;
			float num6 = rigidbody.mass * linearVelocity * linearVelocity / num4;
			float num7 = linearVelocity / num6 * Mathf.Sign(dfState.StartSteer);
			if (Time.time - dfState.StartTime <= (float)carModel.Rotate0To90Time && dfState.Stage == DriftStage.SLIDING)
			{
				num7 += calExtraRot(dfState.StartTime, dfState.StartSteer, ROTATE_ACCEL_TYPE) * (float)DRIFT_SLIDING_ROTATE_FACTOR;
			}
			else if (dfState.Stage == DriftStage.CIRCLING || dfState.Stage == DriftStage.ROTATING)
			{
				num7 += (float)DRIFT_ROTATING_ROTATE_FACTOR * Mathf.Sign(dfState.StartSteer) * dfState.veldot;
			}
			vt1.Set(0f, 1f, 0f);
			carState.rigidbody.rotation *= Quaternion.AngleAxis(num7 * 57.29578f * Time.deltaTime, vt1);
			UpdatePassAngle();
			if (carState.relativeVelocity.z < (float)MIN_DRIFT_SPEED)
			{
				totalForce += CarController.CalculateEngineForce(carModel, carState.relativeVelocity.z / (float)MIN_DRIFT_SPEED, 0) * Time.deltaTime * carState.ThrottleDirection * DRIFT_ENGINE_FROCE_FACTOR;
			}
		}

		private void toRemain(DriftState dfState)
		{
			dfState.Stage = DriftStage.REMAIN;
			dfState.RemainStartAt = Time.time;
			if (dfState.NoRemainFlag == 0)
			{
				dfState.FullRemainTime = carModel.RemainFixtime * Mathf.Clamp01((Time.time - dfState.StartTime) / (float)carModel.Rotate0To90Time);
			}
			else
			{
				dfState.FullRemainTime = 0.2f;
			}
			if (RaceConfig.LeanCameraOnDrift && CameraController.Current != null && CameraController.Current.ViewTarget == carState)
			{
				CameraController.Current.StartLeanTo(0f, RaceConfig.CamLeanEndFactor);
			}
			if (carState.Steer != 0f)
			{
				_triggedDragDrift = true;
				if (carState.CallBacks.OnDragDriftStart != null)
				{
					carState.CallBacks.OnDragDriftStart(carState);
				}
			}
		}

		private void applyDrift()
		{
			DriftState curDriftState = carState.CurDriftState;
			if (!carState.Drift)
			{
				if (curDriftState.Stage == DriftStage.NONE)
				{
					return;
				}
				if (curDriftState.Stage != DriftStage.REMAIN)
				{
					toRemain(curDriftState);
				}
			}
			if (curDriftState.Stage == DriftStage.NONE)
			{
				if (!(carState.SpeedRatio < carModel.MinDriftableSpeedRatio) && !(carState.Throttle <= 0f) && carState.Steer != 0f && carState.CollisionDirection == HitWallDirection.NONE && !(Time.time - carState.HitTime < 0.5f))
				{
					driftStart();
				}
				return;
			}
			if (carState.CallBacks.OnDrift != null)
			{
				carState.CallBacks.OnDrift(CarEventState.EVENT_DOING);
			}
			curDriftState.TimePast += Time.deltaTime;
			onStage(curDriftState);
			carState.LastRightDirTime = Time.time;
		}

		private float calExtraRot(float StartTime, float StartSteer, int type)
		{
			switch (type)
			{
				case 0:
					return (float)Math.PI * (Time.time - StartTime) / ((float)carModel.Rotate0To90Time * (float)carModel.Rotate0To90Time) * Mathf.Sign(StartSteer);
				case 1:
					return 4.712389f * (Time.time - StartTime) * (Time.time - StartTime) / ((float)carModel.Rotate0To90Time * (float)carModel.Rotate0To90Time * (float)carModel.Rotate0To90Time) * Mathf.Sign(StartSteer);
				case 2:
				{
					float num2 = Time.time - StartTime;
					if (num2 <= t1)
					{
						return num2 * a1 * Mathf.Sign(StartSteer);
					}
					num2 = Mathf.Clamp((float)carModel.Rotate0To90Time - num2, 0f, t2);
					return num2 * a2 * Mathf.Sign(StartSteer);
				}
				case 3:
				{
					float num = Time.time - StartTime;
					if (num <= t1)
					{
						return num * a1 * Mathf.Sign(StartSteer);
					}
					return t1 * a1 * Mathf.Sign(StartSteer);
				}
				case 4:
					return (r0 + a1 * (Time.time - StartTime)) * Mathf.Sign(StartSteer);
				default:
					return 0f;
			}
		}

		private float calCircleToRotateRatio(CarModel cm)
		{
			return (float)CIRCLE_TO_ROTATE_SPEED / (float)cm.MaxSpeeds[0];
		}

		private float calRotateToCircleRatio(CarModel cm)
		{
			return (float)ROTATE_TO_CIRCLE_SPEED / (float)cm.MaxSpeeds[0];
		}

		public static float CalculateDriftForce(CarModel param)
		{
			return 0f;
		}
	}
}
