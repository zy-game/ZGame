using System;
using System.Collections.Generic;
using UnityEngine;

namespace CarLogic
{
	[Serializable]
	public class SyncMoveControllerNew : ControllerBase, IResetChecker
	{
		private const int OUT_CRASH_TIME = 2;

		public static float AsyncDelayTime = 0.2f;

		private SyncTime syncTime = new SyncTime();

		private readonly SyncMoveQueue syncMoveQueue = new SyncMoveQueue();

		public SyncMoveSender SyncMoveSender;

		public static float OutSmoothTime = 0.4f;

		public float IgnoreAngle = 1f;

		public float JumpAngle = 12f;

		public bool VelocityRemain = true;

		public bool UseRigidbody;

		private MovementData buf1;

		private MovementData buf2;

		private bool hasTranslateInfo;

		private readonly List<Vector3> listVec = new List<Vector3>();

		private int orz;

		private Vector3 lastForecastPos = Vector3.zero;

		private Vector3 totalForecastDistance = Vector3.zero;

		private const float FORCE_TIME = 1f;

		private Transform me;

		private CharacterController controller;

		private CarState carState;

		private CarModel carModel;

		private CarView view;

		private int lastN2Level;

		private SpecialType _lastSpecialType;

		private DriftStage _lastDriftState;

		private Vector3 vt = Vector3.zero;

		private Vector3 vt2 = Vector3.zero;

		private Vector3 vt3 = Vector3.zero;

		private Vector3 gNormal = Vector3.zero;

		private Vector3 velocity = Vector3.zero;

		private Vector3 accel = Vector3.zero;

		private float smoothTime;

		private float canNotCrashTime;

		private float beginCrashTime;

		private float lastSteer;

		private MovementData CurTarget
		{
			get
			{
				return buf1;
			}
			set
			{
				if (buf1 != null)
				{
					buf2 = buf1;
				}
				buf1 = value;
			}
		}

		public Action<byte[]> OnSelfCarSendAction
		{
			get
			{
				return SyncMoveSender.OnSelfCarSendAction;
			}
			set
			{
				SyncMoveSender.OnSelfCarSendAction = value;
			}
		}

		public object ResetUserData => null;

		public float ResetDelay => 0f;

		public int GetSyncMoveQueueCount()
		{
			return syncMoveQueue.Count;
		}

		public void ClearSyncMoveQueue()
		{
			syncMoveQueue.Clear();
		}

		public void Init(CarView view, CarState state, CarModel model)
		{
			SyncMoveSender = new SyncMoveSender(state);
			carState = state;
			carModel = model;
			me = carState.transform;
			this.view = view;
			controller = carState.view.AddCharacterController();
			if (carState.CarPlayerType == PlayerType.PLAYER_SELF)
			{
				view.OnFixedupdate = (Action)Delegate.Combine(view.OnFixedupdate, new Action(Update));
				CarCallBack callBacks = view.carState.CallBacks;
				callBacks.OnCurNodeChanged = (Action<RacePathManager>)Delegate.Combine(callBacks.OnCurNodeChanged, new Action<RacePathManager>(OnCurNodeChanged));
				controller.enabled = false;
			}
			else if (carState.CarPlayerType == PlayerType.PLAYER_OTHER)
			{
				Debug.LogError("Modify by Sbin: SyncMoveControllerNew: dont SetConstraints");
				view.OnFixedupdate = (Action)Delegate.Combine(view.OnFixedupdate, new Action(Update));
				InitPlayerOther();
				controller.enabled = false;
			}
			else
			{
				Debug.LogWarning($"unknow CarPlayerType: {carState.CarPlayerType}");
			}
		}

		private void InitPlayerOther()
		{
			if (UseRigidbody && me.GetComponent<Rigidbody>() == null)
			{
				me.gameObject.AddComponent<Rigidbody>();
			}
			if (!UseRigidbody)
			{
				if (controller == null)
				{
					controller = carState.view.AddCharacterController();
				}
			}
			else
			{
				if (controller != null)
				{
					controller.enabled = false;
				}
				for (int i = 0; i < carState.Wheels.Length; i++)
				{
					Wheel wheel = carState.Wheels[i];
					if (wheel.collider != null)
					{
						JointSpring suspensionSpring = wheel.collider.suspensionSpring;
						suspensionSpring.spring = 12000f;
						wheel.collider.suspensionSpring = suspensionSpring;
					}
				}
			}
			Collider[] bodyColliders = carState.view.BodyColliders;
			if (bodyColliders != null && bodyColliders.Length != 0)
			{
				for (int j = 0; j < bodyColliders.Length; j++)
				{
					if (bodyColliders[j] != null)
					{
						if (controller != null && controller.enabled)
						{
							Physics.IgnoreCollision(controller, bodyColliders[j]);
						}
						PhysicMaterial material = bodyColliders[j].material;
						if (material != null)
						{
							material.bounciness = 0f;
							material.dynamicFriction = 0f;
							material.staticFriction = 0f;
							material.bounceCombine = PhysicMaterialCombine.Minimum;
						}
					}
				}
			}
			view.Controller.ResizeBoxCollider();
			IgnoreAngle = 3f;
			smoothTime = OutSmoothTime;
		}

		public override void OnActiveChange(bool active)
		{
			if (!(view == null) && !base.Active)
			{
				MovementData.ClearEfState(carState);
			}
		}

		private void CheckNormal()
		{
			if (carState.view != null && carState.Visible)
			{
				carState.view.Controller.setWheelState(carState);
				carState.view.Controller.calGroundNormal();
			}
			else
			{
				carState.WheelHitState = 15;
				carState.GroundNormal.Set(0f, 1f, 0f);
			}
		}

		private void Update()
		{
			if (base.Active)
			{
				if (carState.CarPlayerType == PlayerType.PLAYER_SELF)
				{
					SyncMoveSender.OnUpdateSelf();
				}
				else if (carState.CarPlayerType == PlayerType.PLAYER_OTHER)
				{
					CheckNormal();
					gNormal = carState.view.GetGNormalByNode(carState.transform.position);
					OnUpdateSync();
				}
			}
		}

		private void OnCurNodeChanged(RacePathManager pathManager)
		{
			if (!(carState.CurNode == null) && !(carState.CurNode.LeftNode == null))
			{
				RacePathNode leftNode = carState.CurNode.LeftNode;
				if (leftNode != null && leftNode == pathManager.EndNode && (float)carState.PastDistance > pathManager.TotalLength * 0.8f)
				{
					syncMoveQueue.RepairTimeDelay();
				}
			}
		}

		private void ClampVelocity(MovementData target)
		{
			if (target != null && VelocityRemain)
			{
				if (UseRigidbody)
				{
					if ((bool)me.GetComponent<Rigidbody>() && !me.GetComponent<Rigidbody>().isKinematic)
					{
						me.GetComponent<Rigidbody>().velocity = target.Velocity;
					}
				}
				else
				{
					velocity = target.Velocity;
					accel = target.AccelVelocity;
				}
			}
			else
			{
				if (VelocityRemain)
				{
					return;
				}
				if (UseRigidbody)
				{
					if ((bool)me.GetComponent<Rigidbody>() && !me.GetComponent<Rigidbody>().isKinematic)
					{
						me.GetComponent<Rigidbody>().velocity = Vector3.zero;
					}
				}
				else
				{
					velocity.Set(0f, 0f, 0f);
					accel.Set(0f, 0f, 0f);
				}
			}
		}

		private void OnUpdateSync()
		{
			syncTime.ClearCache();
			syncMoveQueue.MarkSyncTime(syncTime);
			if (syncMoveQueue.Count == 0)
			{
				return;
			}
			syncMoveQueue.InitBeginTime(syncTime, AsyncDelayTime);
			float time = syncTime.time;
			if (time < syncMoveQueue.BeginTime)
			{
				return;
			}
			if (view.RunState == RunState.Run)
			{
				beginCrashTime = 0f;
				syncTime.ClearEase();
			}
			List<MovementData> list = syncMoveQueue.DelOverTime();
			for (int i = 0; i < list.Count; i++)
			{
				MakeSpecialCs(list[i]);
			}
			if (syncMoveQueue.Count == 0)
			{
				return;
			}
			MakeSpecialCs(syncMoveQueue.FirstMovement);
			if (carState.ApplyingSpecialType == SpecialType.Translate)
			{
				syncMoveQueue.DelQueueHead();
				return;
			}
			if (syncMoveQueue.IsTimeBetweenFirstTwo(time))
			{
				CurTarget = syncMoveQueue.FirstMovement;
				carState.rigidbody.velocity = Vector3.zero;
				smoothTime += syncTime.deltaTime;
				if (smoothTime > OutSmoothTime || syncMoveQueue.IsZeroVelocity())
				{
					UpdatePos();
					UpdateRotation();
					UpdateWheelRot();
					UpdateCarLinearVelocity();
				}
				else
				{
					SmoothPos();
					ForecastRot(syncMoveQueue.SecondMovement);
					UpdateWheelRot();
					UpdateCarLinearVelocity();
				}
				return;
			}
			smoothTime = 0f;
			CurTarget = syncMoveQueue.FirstMovement;
			AutoFallingGround();
			if (!syncMoveQueue.IsZeroVelocity() && time >= syncMoveQueue.CurrTime)
			{
				carState.rigidbody.velocity = Vector3.zero;
				ForecastPos();
				ForecastRot(CurTarget);
			}
			else
			{
				Vector3 vector = CurTarget.Position - carState.view.transform.position;
				float magnitude = vector.magnitude;
				if (magnitude > 1f)
				{
					carState.rigidbody.velocity = vector.normalized * magnitude / (CurTarget.DeltaTime() + AsyncDelayTime);
				}
				else
				{
					carState.rigidbody.velocity = Vector3.zero;
				}
			}
			UpdateCarLinearVelocity();
			carState.view.RotateAngleForCarModel(CurTarget.CarModelRotation);
		}

		private void UpdateCarLinearVelocity()
		{
			if (CurTarget != null && CurTarget.Speed > 0f)
			{
				Vector3 vector = carState.view.transform.InverseTransformDirection(CurTarget.Velocity);
				carState.LinearVelocity = vector.magnitude;
				carState.SpeedRatio = Mathf.Clamp01(vector.z / Mathf.Max(carState.view.carModel.MaxSpeeds[0], 1f));
			}
		}

		private void AutoFallingGround()
		{
			if (CurTarget == null || !CurTarget.HasTranslateInfo)
			{
				Vector3 position = FallingToTheGround(carState.view.transform.position);
				carState.view.GetComponent<Rigidbody>().MovePosition(position);
			}
		}

		private void UpdateCarPosition()
		{
			if (view.RunState == RunState.Crash && canNotCrashTime > 2f)
			{
				me.position = vt;
			}
			carState.rigidbody.MovePosition(vt);
		}

		private void ForecastRot(MovementData curTarget)
		{
			Vector3 eularAngle = carState.eularAngle;
			Vector3 eularAngle2 = curTarget.EularAngle;
			Vector3 zero = Vector3.zero;
			bool flag = false;
			for (int i = 0; i < 3; i++)
			{
				zero[i] = Mathf.LerpAngle(eularAngle[i], eularAngle2[i], 0.08f);
			}
			if (Mathf.Abs(Mathf.DeltaAngle(eularAngle[0], eularAngle2[0])) < IgnoreAngle)
			{
				zero[0] = carState.eularAngle.x;
			}
			carState.eularAngle = zero;
			carState.transform.rotation = Quaternion.Euler(zero);
		}

		private void ForecastPos()
		{
			MovementData firstMovement = syncMoveQueue.FirstMovement;
			vt2 = firstMovement.Velocity;
			vt2.y = ((vt2.y > -0.1f && vt2.y < 0.1f) ? 0f : vt2.y);
			float angleSpeed = firstMovement.AngleSpeed;
			Quaternion quaternion = Quaternion.AngleAxis(angleSpeed, carState.transform.up);
			vt2 = quaternion * vt2;
			float num = 1f;
			if (syncMoveQueue.HasNewData && syncTime.time > syncMoveQueue.CurrTime + 1f)
			{
				if (Vector3.Distance(firstMovement.Position, me.position) > 5f)
				{
					vt = firstMovement.Position;
					carState.eularAngle = firstMovement.Rotation.eulerAngles;
				}
				else
				{
					vt2.x = ((vt2.x > -0.1f && vt2.x < 0.1f) ? 0f : vt2.x);
					vt2.z = ((vt2.z > -0.1f && vt2.z < 0.1f) ? 0f : vt2.z);
					vt = me.position + vt2 * syncTime.deltaTime * num;
				}
				velocity = Vector3.zero;
				lastForecastPos = vt;
				syncMoveQueue.HasNewData = false;
				totalForecastDistance = Vector3.zero;
			}
			else if (syncMoveQueue.HasNewData)
			{
				float num2 = 1f - syncTime.time + syncTime.deltaTime + syncMoveQueue.CurrTime;
				vt = firstMovement.Position + vt2 * 1f * num;
				accel = (vt - me.position - velocity * num2) * 2f / num2 / num2;
				vt = me.position + velocity * syncTime.deltaTime + 0.5f * accel * syncTime.deltaTime * syncTime.deltaTime;
				velocity += accel * syncTime.deltaTime;
				lastForecastPos = vt;
				syncMoveQueue.HasNewData = false;
				totalForecastDistance = Vector3.zero;
			}
			else if (syncTime.time > syncMoveQueue.CurrTime + 1f)
			{
				lastForecastPos.y = me.position.y;
				totalForecastDistance += me.position - lastForecastPos;
				if (totalForecastDistance.magnitude > 0.1f)
				{
					num = 0.6f * num;
				}
				if (totalForecastDistance.magnitude > 20f)
				{
					num *= 1f - totalForecastDistance.magnitude / 100f;
				}
				num = ((num > 0f) ? num : 0f);
				vt2.x = ((vt2.x > -0.1f && vt2.x < 0.1f) ? 0f : vt2.x);
				vt2.z = ((vt2.z > -0.1f && vt2.z < 0.1f) ? 0f : vt2.z);
				vt = me.position + vt2 * syncTime.deltaTime * num;
			}
			else
			{
				lastForecastPos.y = me.position.y;
				totalForecastDistance += me.position - lastForecastPos;
				if (totalForecastDistance.magnitude > 0.1f)
				{
					num = 0.6f * num;
				}
				float num3 = 1f - syncTime.time + syncTime.deltaTime + syncMoveQueue.CurrTime;
				vt = firstMovement.Position + vt2 * 1f * num + totalForecastDistance;
				accel = (vt - me.position - velocity * num3) * 2f / num3 / num3;
				vt = me.position + velocity * syncTime.deltaTime + 0.5f * accel * syncTime.deltaTime * syncTime.deltaTime;
				velocity += accel * syncTime.deltaTime;
			}
			vt = FallingToTheGround(vt);
			lastForecastPos = vt;
			UpdateCarPosition();
		}

		private void SyncN2State()
		{
			if (_lastSpecialType != carState.ApplyingSpecialType && (carState.ApplyingSpecialType == SpecialType.SpeedUp || carState.ApplyingSpecialType == SpecialType.SustainedSpeedUp) && carState.CallBacks.OnGas != null)
			{
				carState.CallBacks.OnGas(CarEventState.EVENT_BEGIN);
			}
			if (_lastSpecialType != carState.ApplyingSpecialType || lastN2Level != carState.N2State.Level)
			{
				if (carState.N2State.Level == 1 && ((carState.N2State.GasType != N2StateGasType.COUPLE && carState.N2State.GasType != N2StateGasType.CUPID) || carState.ApplyingSpecialType == SpecialType.SpeedUp))
				{
					CommonGasItem commonGasItem = new CommonGasItem(1, carState.view.carModel.N2ForceTime, useShake: false);
					commonGasItem.IsTeam = carState.N2State.GasType == N2StateGasType.TEAM;
					carState.Items.Add(commonGasItem);
					if (carState.CallBacks.OnAffectedByItem != null)
					{
						carState.CallBacks.OnAffectedByItem(commonGasItem.ItemId, carState, ItemCallbackType.AFFECT, commonGasItem);
						carState.AnimationFlag = AnimStage.BigHighSpeed;
					}
					if (carState.CallBacks.OnGas != null)
					{
						carState.CallBacks.OnGas(CarEventState.EVENT_BEGIN);
					}
				}
				else
				{
					List<RaceItemBase> list = new List<RaceItemBase>();
					if (carState.view.ItController.ApplyingItem(RaceItemId.GAS, list) || carState.view.ItController.ApplyingItem(RaceItemId.GROUP_GAS, list))
					{
						carState.Items.Clear();
						if (carState.CallBacks.OnAffectedByItem != null && list.Count > 0 && list[0] != null)
						{
							carState.CallBacks.OnAffectedByItem(list[0].ItemId, carState, ItemCallbackType.BREAK, list[0]);
							carState.AnimationFlag = AnimStage.CommonDrive;
						}
						if (carState.CallBacks.OnGas != null)
						{
							carState.CallBacks.OnGas(CarEventState.EVENT_END);
						}
					}
					if (carState.N2State.Level > 1 && carState.CallBacks.OnGas != null)
					{
						carState.CallBacks.OnGas(CarEventState.EVENT_BEGIN);
					}
				}
				if (carState.N2State.Level > 1)
				{
					carState.AnimationFlag = AnimStage.HighSpeed;
				}
				else if (carState.N2State.Level < 1)
				{
					carState.AnimationFlag = AnimStage.CommonDrive;
				}
			}
			else if (carState.N2State.Level > 0 && carState.CallBacks.OnGas != null)
			{
				carState.CallBacks.OnGas(CarEventState.EVENT_DOING);
			}
		}

		private void SyncDriftStage()
		{
			if (_lastDriftState != carState.CurDriftState.Stage && carState.CallBacks.OnDrift != null)
			{
				CarEventState obj = CarEventState.EVENT_DOING;
				if (_lastDriftState == DriftStage.NONE)
				{
					obj = CarEventState.EVENT_BEGIN;
				}
				if (carState.CurDriftState.Stage == DriftStage.NONE)
				{
					obj = CarEventState.EVENT_END;
				}
				carState.CallBacks.OnDrift(obj);
			}
		}

		private bool SyncTranslateInfo(MovementData curTarget)
		{
			if (curTarget != null && curTarget.HasTranslateInfo)
			{
				if (!hasTranslateInfo)
				{
					ToggleTranslate(curTarget);
				}
				carState.TransQTEok = curTarget.TranslateQteOk;
				carState.TransQteAnimIndex = curTarget.TranslateQteAnimationIndex;
			}
			hasTranslateInfo = curTarget.HasTranslateInfo;
			return hasTranslateInfo;
		}

		private bool MakeSpecialCs(MovementData curTarget)
		{
			if (curTarget != null)
			{
				lastN2Level = carState.N2State.Level;
				_lastSpecialType = carState.ApplyingSpecialType;
				_lastDriftState = carState.CurDriftState.Stage;
				curTarget.GetEfState(carState);
				curTarget.GetItems(carState);
				curTarget.GetScrapeValue(carState);
				SyncN2State();
				SyncDriftStage();
			}
			return SyncTranslateInfo(curTarget);
		}

		private void OnDrawGizmosSelected()
		{
			orz = (orz + 1) % 255;
			Gizmos.color = new Color(255f, orz, orz);
			for (int i = 0; i < listVec.Count; i++)
			{
				if (i != 0)
				{
					Gizmos.DrawLine(listVec[i - 1], listVec[i]);
				}
			}
			LinkedListNode<MovementData> linkedListNode = syncMoveQueue.First;
			for (int j = 1; j < syncMoveQueue.Count; j++)
			{
				if (j != 1)
				{
					Gizmos.DrawLine(linkedListNode.Value.Position, linkedListNode.Next.Value.Position);
				}
				linkedListNode = linkedListNode.Next;
			}
		}

		private void SmoothPos()
		{
			float num = syncTime.time + (OutSmoothTime - smoothTime);
			float num2 = syncMoveQueue.CurrTime;
			LinkedListNode<MovementData> last = syncMoveQueue.Last;
			LinkedListNode<MovementData> linkedListNode;
			for (linkedListNode = syncMoveQueue.First; num > num2; num2 += linkedListNode.Value.DeltaTime())
			{
				if (linkedListNode == last)
				{
					break;
				}
				linkedListNode = linkedListNode.Next;
			}
			MovementData value = linkedListNode.Previous.Value;
			MovementData value2 = linkedListNode.Value;
			float num3 = num - num2;
			Vector3 vector3;
			if (num3 < 0f)
			{
				float num4 = value2.DeltaTime() + num3;
				Vector3 vector = value.Velocity;
				Vector3 vector2 = value2.Position - value.Position;
				accel = 2f * (vector2 / value2.DeltaTime() - value.Velocity) / value2.DeltaTime();
				vector3 = value.Position + vector * num4 + 0.5f * accel * num4 * num4;
			}
			else if (linkedListNode.Next != null)
			{
				MovementData value3 = linkedListNode.Next.Value;
				float num5 = num3;
				Vector3 vector4 = value2.Velocity;
				Vector3 vector5 = value3.Position - value2.Position;
				accel = 2f * (vector5 / value3.DeltaTime() - value2.Velocity) / value3.DeltaTime();
				vector3 = value2.Position + vector4 * num5 + 0.5f * accel * num5 * num5;
			}
			else
			{
				vector3 = value2.Position + num3 * value2.Velocity;
			}
			float deltaTime = syncTime.deltaTime;
			float num6 = num - (syncTime.time - deltaTime);
			Vector3 vector6 = velocity;
			accel = (vector3 - me.position - vector6 * num6) * 2f / num6 / num6;
			velocity = vector6 + accel * deltaTime;
			velocity = Vector3.ClampMagnitude(velocity, MaxSpeed());
			accel = (velocity - vector6) / deltaTime;
			vt = me.position + vector6 * deltaTime + 0.5f * accel * deltaTime * deltaTime;
			vt = FallingToTheGround(vt);
			UpdateCarPosition();
		}

		private float MaxSpeed()
		{
			if (carState.N2State.Level != 1)
			{
				return carModel.MaxSpeedsKmph[0];
			}
			float num = 0f;
			Float[] maxSpeedsKmph = carModel.MaxSpeedsKmph;
			Float[] array = maxSpeedsKmph;
			foreach (Float @float in array)
			{
				num = Mathf.Max(num, @float);
			}
			return num;
		}

		private Vector3 FallingToTheGround(Vector3 pos)
		{
			if (CurTarget.HasTranslateInfo)
			{
				return pos;
			}
			CharacterController component = carState.view.GetComponent<CharacterController>();
			if ((bool)component && Physics.Raycast(pos + new Vector3(0f, 2f, 0f), Vector3.down, out var hitInfo, 100f, 256))
			{
				Vector3 point = hitInfo.point;
				point.y += 0f - component.center.y + component.radius + 0.05f;
				if (point.y + 0.3f >= pos.y)
				{
					pos = point;
				}
			}
			return pos;
		}

		private void UpdateCurPosLerpWithoutVelocity(Vector3 beginPos, Vector3 endPos, float totalTime)
		{
			Debug.DrawLine(beginPos, endPos, Color.red);
			vt = Vector3.Lerp(beginPos, endPos, (syncTime.time - syncMoveQueue.CurrTime) / totalTime);
			UpdateCarPosition();
			velocity = Vector3.zero;
			accel = Vector3.zero;
		}

		private void UpdatePosByAccelKinematics(Vector3 beginPos, Vector3 endPos, Vector3 beginVel, float curTime, float totalTime)
		{
			Vector3 vector = endPos - beginPos;
			accel = 2f * (vector - beginVel * totalTime) / totalTime / totalTime;
			vt = beginPos + beginVel * curTime + 0.5f * accel * curTime * curTime;
			CharacterController component = carState.view.GetComponent<CharacterController>();
			if ((bool)component && Physics.Raycast(vt + new Vector3(0f, 2f, 0f), Vector3.down, out var hitInfo, 100f, 256))
			{
				Vector3 point = hitInfo.point;
				point.y += 0f - component.center.y + component.radius + 0.05f;
				if (point.y >= Mathf.Min(beginPos.y, endPos.y) && point.y <= Mathf.Max(beginPos.y, endPos.y) && point.y >= vt.y)
				{
					vt = point;
				}
			}
			if (vt.y + 0.02f < Mathf.Min(beginPos.y, endPos.y))
			{
				vt.y = Mathf.Min(beginPos.y, endPos.y);
			}
			velocity = beginVel + accel * curTime;
			UpdateCarPosition();
			carState.velocity = velocity;
			carState.relativeVelocity = carState.transform.InverseTransformDirection(carState.velocity);
		}

		private void UpdatePos()
		{
			MovementData firstMovement = syncMoveQueue.FirstMovement;
			MovementData secondMovement = syncMoveQueue.SecondMovement;
			if (firstMovement.Speed <= 0f && secondMovement.Speed <= 0f)
			{
				UpdateCurPosLerpWithoutVelocity(firstMovement.Position, secondMovement.Position, secondMovement.DeltaTime());
				return;
			}
			float curTime = syncTime.time - syncMoveQueue.CurrTime;
			UpdatePosByAccelKinematics(firstMovement.Position, secondMovement.Position, firstMovement.Velocity, curTime, secondMovement.DeltaTime());
		}

		private void UpdateRotation()
		{
			MovementData firstMovement = syncMoveQueue.FirstMovement;
			MovementData secondMovement = syncMoveQueue.SecondMovement;
			float value = syncTime.time - syncMoveQueue.CurrTime;
			Vector3 zero = Vector3.zero;
			for (int i = 0; i < 3; i++)
			{
				zero[i] = Mathf.LerpAngle(firstMovement.EularAngle[i], secondMovement.EularAngle[i], Mathf.InverseLerp(0f, secondMovement.DeltaTime(), value));
			}
			if (Mathf.Abs(Mathf.DeltaAngle(firstMovement.EularAngle[0], secondMovement.EularAngle[0])) < IgnoreAngle)
			{
				zero[0] = carState.eularAngle.x;
			}
			me.rotation = Quaternion.Euler(zero);
			carState.eularAngle = zero;
			carState.transform.rotation = me.rotation;
		}

		private float CalcSteer()
		{
			float f = Vector3.Dot((syncMoveQueue.SecondMovement.Velocity - syncMoveQueue.FirstMovement.Velocity).normalized, view.transform.right);
			return ((double)Mathf.Abs(f) <= 0.1) ? 0f : Mathf.Sign(f);
		}

		private void UpdateWheelRot()
		{
			carState.view.CheckSteer();
		}

		public void UpdateWheelGraphics(Vector3 relativeVelocity)
		{
			if (carState == null || carState.Wheels == null || carModel == null)
			{
				return;
			}
			Quaternion b = ((carState.Steer < 0f) ? Quaternion.Euler(0f, -35f, 0f) : Quaternion.Euler(0f, 35f, 0f));
			Wheel[] wheels = carState.Wheels;
			Wheel[] array = wheels;
			foreach (Wheel wheel in array)
			{
				if (wheel != null && !(wheel.collider == null) && (bool)wheel.tireGraphic && wheel.steerWheel)
				{
					Transform parent = wheel.tireGraphic.transform.parent;
					Quaternion quaternion2 = (parent.localRotation = Quaternion.Lerp(parent.localRotation, b, Mathf.Clamp01(Time.time - carState.SteerChangeTime) / (float)carModel.FullRotateTime * 5f));
					b = quaternion2;
				}
			}
		}

		private bool ToggleTranslate(MovementData data)
		{
			int translateStarterId = data.TranslateStarterId;
			int translatePathIndex = data.TranslatePathIndex;
			bool result = false;
			if (RacePathManager.ActiveInstance == null)
			{
				return result;
			}
			TranslateStarter value = null;
			if (RacePathManager.ActiveInstance.TranslateMap.TryGetValue(translateStarterId, out value))
			{
				if (value != null && value.Paths != null && translatePathIndex < value.Paths.Length && translatePathIndex >= 0)
				{
					TranslatePath translatePath = value.Paths[translatePathIndex];
					if (translatePath != null)
					{
						TranslateTrigger translateTrigger = new TranslateTrigger(translatePath, data.TranslateTime, value.EndTrigger, value.PushGrounded, value.TimeCurve);
						translateTrigger.AddCallback(delegate(SpecialTriggerBase st, SpecialCallback sc)
						{
							if (sc == SpecialCallback.Stop)
							{
								velocity = carState.velocity;
							}
						});
						translateTrigger.Toggle(carState);
						velocity = Vector3.zero;
					}
				}
			}
			else
			{
				Debug.LogWarning("No Translate Starter of id found: " + translateStarterId);
			}
			return result;
		}

		public void AsyncMovement(byte[] bytes, bool forceSet)
		{
			if (carState != null)
			{
				if (carState.CarPlayerType == PlayerType.PLAYER_OTHER)
				{
					MovementData data = new MovementData(bytes);
					syncMoveQueue.AddQueueEnd(data);
				}
				else if (carState.CarPlayerType == PlayerType.PLAYER_SELF)
				{
					MovementData data2 = new MovementData(bytes);
					SyncMoveSender.UpdateNetDelay(data2);
				}
			}
		}

		public void AsyncMovement(MovementData data, bool forceSet)
		{
			if (carState != null)
			{
				if (carState.CarPlayerType == PlayerType.PLAYER_OTHER)
				{
					syncMoveQueue.AddQueueEnd(data);
				}
				else if (carState.CarPlayerType == PlayerType.PLAYER_SELF)
				{
					SyncMoveSender.UpdateNetDelay(data);
				}
			}
		}

		public bool StartToWait()
		{
			if (!base.Active)
			{
				return false;
			}
			return me.position.y < -20f;
		}

		public bool Cancelable(object data)
		{
			return false;
		}

		public bool NeedReset(object userdata)
		{
			return StartToWait();
		}

		public void OnReset()
		{
			MovementData movementData = CurTarget ?? buf2;
			if (movementData != null)
			{
				me.position = movementData.Position;
				me.rotation = movementData.Rotation;
				carState.view.RotateAngleForCarModel(movementData.CarModelRotation);
				carState.view.ForceGround();
				movementData.GetEfState(carState);
				movementData.GetItems(carState);
				movementData.GetScrapeValue(carState);
				ClampVelocity(movementData);
			}
		}
	}
}
