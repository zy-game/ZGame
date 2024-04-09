using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CarLogic
{
	[Serializable]
	public class SyncMoveControllerClassic : ControllerBase, IResetChecker
	{
		public float AsyncInterval = 0.5f;

		public float IgnoreDistance = 0.2f;

		public float IgnoreAngle = 1f;

		public float MaxJumpDistance = 20f;

		public float MinJumpDistance = 5f;

		public float jumpDistance = 20f;

		public float JumpAngle = 12f;

		public float AsyncRotateSpeed = 5f;

		public bool VelocityRemain = true;

		public bool UseRigidbody;

		public bool UseQueue;

		public bool UseEmulateTarget;

		public bool UseAccel = true;

		public const bool ApplyExtraGravity = true;

		public bool JointMove;

		private bool ForceUpward = true;

		private bool lookPath = true;

		private bool linearMove = true;

		public float EmulateFactor;

		public static float PushFactor = 1f;

		internal long NetDelay;

		private Dictionary<uint, long> netTimer = new Dictionary<uint, long>();

		private float timeLeft;

		private LinkedList<MovementData> pathQueue = new LinkedList<MovementData>();

		private MovementData buf1;

		private MovementData buf2;

		private Transform me;

		private CharacterController controller;

		private ushort myIndex;

		private CarState carState;

		private CarModel carModel;

		private CarView view;

		private float ignoreDot;

		private float jumpDot;

		private float moveSpeed = 1f;

		private Vector3 emuTarget = Vector3.zero;

		private float oldThrottle;

		private float oldSpeed;

		private int lastGHit;

		private float lastSendTime;

		private int lastN2Level;

		private SpecialType lastStype;

		private Rigidbody jRigidbody;

		private SpringJoint joint;

		private const int ROTATION_ADJUST_INTERVAL = 5;

		private float velSyncDot;

		private Vector3 vt = Vector3.zero;

		private Vector3 vt2 = Vector3.zero;

		private Vector3 vt3;

		private Vector3 gNormal;

		private float jumpTmp;

		private Vector3 velocity = Vector3.zero;

		private Vector3 accel = Vector3.zero;

		private static int losePackages = 0;

		private static int manulCast = 0;

		private Vector3 emuPosition;

		private Vector3 gravity = new Vector3(0f, -30f, 0f);

		private System.Random ran = new System.Random();

		private Vector2 p0;

		private Vector2 p1;

		private Vector2 p2;

		private Vector2 p3;

		private Vector2 tp;

		private float pStartTime;

		private float duration = 1f;

		private Vector3 vforward = Vector3.forward;

		private bool hasTranslateInfo;

		private Transform ttrans;

		public Action<byte[]> OnSelfCarSendAction;

		private MovementData curTarget
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

		internal Transform dTrans
		{
			get
			{
				if (ttrans == null)
				{
					ttrans = new GameObject(view.name + "-dTrans").transform;
				}
				return ttrans;
			}
		}

		public object ResetUserData => null;

		public float ResetDelay => 0f;

		public int GetSyncMoveQueueCount()
		{
			return pathQueue.Count;
		}

		public void ClearSyncMoveQueue()
		{
			pathQueue.Clear();
		}

		public void Init(CarView view, CarState state, CarModel model)
		{
			carState = state;
			carModel = model;
			me = carState.transform;
			this.view = view;
			controller = carState.view.AddCharacterController();
			if (carState.CarPlayerType != PlayerType.PLAYER_OTHER)
			{
				if (controller != null && carState.CarPlayerType == PlayerType.PLAYER_SELF)
				{
					controller.enabled = false;
				}
				return;
			}
			ignoreDot = Mathf.Cos(IgnoreAngle * ((float)Math.PI / 180f));
			jumpDot = Mathf.Cos(JumpAngle * ((float)Math.PI / 180f));
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
			if (bodyColliders == null || bodyColliders.Length == 0)
			{
				return;
			}
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

		public void RemovePlayer()
		{
			if (!(me == null))
			{
				UnityEngine.Object.Destroy(me.gameObject);
			}
		}

		public void ResetTimeLeft()
		{
			timeLeft = 0f;
		}

		public override void OnActiveChange(bool active)
		{
			if (view == null)
			{
				return;
			}
			CarView carView = view;
			carView.AcOnDrawGizmosSelected = (Action)Delegate.Remove(carView.AcOnDrawGizmosSelected, new Action(onDrawGizmo));
			CarView carView2 = view;
			carView2.OnFixedupdate = (Action)Delegate.Remove(carView2.OnFixedupdate, new Action(Update));
			CarView carView3 = view;
			carView3.OnUpdate = (Action)Delegate.Remove(carView3.OnUpdate, new Action(Update));
			CarCallBack callBacks = carState.CallBacks;
			callBacks.OnSpecialCallback = (Action<CarState, SpecialTriggerBase, SpecialCallback>)Delegate.Remove(callBacks.OnSpecialCallback, new Action<CarState, SpecialTriggerBase, SpecialCallback>(onSynTranslateCallback));
			breakJoint();
			if (active)
			{
				carView.AcOnDrawGizmosSelected = (Action)Delegate.Combine(carView.AcOnDrawGizmosSelected, new Action(onDrawGizmo));
				if ((!UseRigidbody && !JointMove) || carState.CarPlayerType == PlayerType.PLAYER_SELF)
				{
					CarView carView4 = view;
					carView4.OnUpdate = (Action)Delegate.Combine(carView4.OnUpdate, new Action(Update));
				}
				else
				{
					CarView carView5 = view;
					carView5.OnFixedupdate = (Action)Delegate.Combine(carView5.OnFixedupdate, new Action(Update));
				}
				if (carState.CarPlayerType != PlayerType.PLAYER_OTHER)
				{
					return;
				}
				CarCallBack callBacks2 = carState.CallBacks;
				callBacks2.OnSpecialCallback = (Action<CarState, SpecialTriggerBase, SpecialCallback>)Delegate.Combine(callBacks2.OnSpecialCallback, new Action<CarState, SpecialTriggerBase, SpecialCallback>(onSynTranslateCallback));
				if (JointMove)
				{
					if (controller != null)
					{
						controller.enabled = false;
					}
					if (jRigidbody == null)
					{
						view.StartCoroutine(buildTargetRigidbody());
					}
					else
					{
						resetJoint();
					}
				}
			}
			else
			{
				MovementData.ClearEfState(carState);
			}
		}

		private void Update()
		{
			if (base.Active)
			{
				if (carState.CarPlayerType == PlayerType.PLAYER_SELF)
				{
					onUpdateSelf();
				}
				else if (carState.CarPlayerType == PlayerType.PLAYER_OTHER)
				{
					syncUpdate();
				}
			}
		}

		private void onUpdateSelf()
		{
			bool flag = false;
			if (curTarget != null)
			{
				float z = carState.relativeVelocity.z;
				if (Mathf.Sign(z) != Mathf.Sign(oldSpeed) && Mathf.Abs(z + oldSpeed) > 3f)
				{
					flag = true;
				}
			}
			if ((lastGHit == 0 && carState.GroundHit != 0) || (lastGHit != 0 && carState.GroundHit == 0))
			{
				flag = true;
			}
			float y = carState.rigidbody.angularVelocity.y;
			if (AsyncInterval > 0.1f && Mathf.Abs(y) > 1f)
			{
				timeLeft -= Time.deltaTime * 3f;
			}
			if (timeLeft <= 0f || oldThrottle != carState.Throttle)
			{
				oldThrottle = carState.Throttle;
				flag = true;
			}
			if (lastStype != carState.ApplyingSpecialType)
			{
				lastStype = carState.ApplyingSpecialType;
				flag = true;
			}
			if (flag)
			{
				SendTransform();
				timeLeft = AsyncInterval;
				lastGHit = carState.GroundHit;
				oldSpeed = carState.relativeVelocity.z;
			}
			else
			{
				timeLeft -= Time.deltaTime;
			}
		}

		private void syncUpdate()
		{
			if (ForceUpward)
			{
				CheckNormal();
			}
			if ((!UseRigidbody || carState.GroundHit != 0) && !lookPath)
			{
				syncRotation();
			}
			gNormal = carState.view.GetGNormalByNode(carState.rigidbody.position);
			syncUpdateCS();
			if (lookPath)
			{
				syncRotation();
			}
			if (controller == null || !controller.enabled)
			{
				applyGravity();
			}
			if (curTarget != null)
			{
				lastN2Level = carState.N2State.Level;
				curTarget.GetEfState(carState);
				curTarget.GetItems(carState);
				curTarget.GetScrapeValue(carState);
				if (lastN2Level != carState.N2State.Level)
				{
					if (lastN2Level == 0 && carState.N2State.Level == 1)
					{
						RaceItemBase raceItemBase = null;
						raceItemBase = ((!curTarget.ApplyingGroupGas()) ? ((RaceItemBase)new CommonGasItem(1, 4f, useShake: false)) : ((RaceItemBase)new RaceGroupGasItem()));
						carState.Items.Add(raceItemBase);
						if (carState.CallBacks.OnAffectedByItem != null)
						{
							carState.CallBacks.OnAffectedByItem(raceItemBase.ItemId, carState, ItemCallbackType.AFFECT, raceItemBase);
						}
					}
					else if (lastN2Level == 1 && carState.N2State.Level == 0)
					{
						List<RaceItemBase> list = new List<RaceItemBase>();
						if (carState.view.ItController.ApplyingItem(RaceItemId.GAS, list) || carState.view.ItController.ApplyingItem(RaceItemId.GROUP_GAS, list))
						{
							carState.Items.Clear();
							if (carState.CallBacks.OnAffectedByItem != null && list.Count > 0 && list[0] != null)
							{
								carState.CallBacks.OnAffectedByItem(list[0].ItemId, carState, ItemCallbackType.BREAK, list[0]);
							}
						}
					}
				}
			}
			if (velSyncDot < 0.2f)
			{
				jumpDistance = Mathf.SmoothDamp(jumpDistance, MaxJumpDistance, ref jumpTmp, 2f);
			}
			else
			{
				jumpDistance = Mathf.SmoothDamp(jumpDistance, MinJumpDistance, ref jumpTmp, 2f);
			}
		}

		private void syncPosition()
		{
			Vector3 vector = curTarget.Position - me.position;
			float magnitude = vector.magnitude;
			float magnitude2 = velocity.magnitude;
			if (magnitude > jumpDistance)
			{
				nextTarget();
			}
			else if (magnitude > IgnoreDistance)
			{
				vt = vector.normalized;
				float num = 0f;
				num = ((!UseRigidbody) ? Vector3.Dot(vt, velocity.normalized) : Vector3.Dot(vt, me.GetComponent<Rigidbody>().velocity.normalized));
				if (num > 0.7f)
				{
					smoothMove();
				}
				else
				{
					nextTarget(autoSetPosition: false);
					Log("Beyond target");
				}
			}
			else
			{
				nextTarget(autoSetPosition: false);
			}
			if (ShowDebug && curTarget != null)
			{
				Debug.DrawLine(carState.transform.position, curTarget.Position, Color.blue);
			}
		}

		private void syncRotation()
		{
			if (curTarget == null)
			{
				return;
			}
			vt = carState.eularAngle;
			vt2 = curTarget.EularAngle;
			float f = Mathf.DeltaAngle(vt.y, vt2.y);
			if (!lookPath && (Mathf.Abs(f) > JumpAngle || Mathf.Abs(f) < IgnoreAngle))
			{
				LogWarning("Force set angle");
				vt.y = vt2.y;
				carState.rigidbody.rotation = Quaternion.Euler(vt);
				carState.eularAngle = vt;
				AsyncRotateSpeed = 0f;
				return;
			}
			bool flag = false;
			for (int i = 0; i < 3; i++)
			{
				vt3[i] = Mathf.LerpAngle(vt[i], vt2[i], 0.08f);
				flag |= Mathf.DeltaAngle(vt3[i], vt2[i]) > 45f;
			}
			if (lookPath && !flag)
			{
				if (velocity.sqrMagnitude > 1f)
				{
					int num = 0;
					if (carState.CurDriftState.Stage != 0 && curTarget != null)
					{
						num = curTarget.DriftStartSteer;
					}
					vt2 = carState.rigidbody.rotation * vforward;
					carState.velocity = velocity - Vector3.Project(velocity, gNormal);
					if (num < 0)
					{
						vt = Vector3.Cross(carState.velocity, gNormal);
					}
					else if (num > 0)
					{
						vt = Vector3.Cross(gNormal, carState.velocity);
					}
					else
					{
						vt = carState.velocity;
						if (Vector3.Dot(vt, vt2) < -0.1f)
						{
							vt *= -1f;
						}
					}
					Quaternion quaternion = Quaternion.Lerp(carState.rigidbody.rotation, Quaternion.LookRotation(vt, gNormal), Time.deltaTime * 10f);
					quaternion = Quaternion.LookRotation(Vector3.Cross(gNormal, quaternion * Vector3.left), gNormal);
					carState.rigidbody.rotation = quaternion;
				}
				else
				{
					carState.rigidbody.rotation = Quaternion.LookRotation(Vector3.Cross(gNormal, -me.right), gNormal);
				}
			}
			else
			{
				carState.eularAngle = vt3;
				carState.rigidbody.rotation = Quaternion.Euler(vt3);
				LogWarning("Reset rotation " + Time.time);
			}
		}

		private float calOrientYAngle(float targetY, float curY, Vector3 vel, int dfSteer = 0)
		{
			float num = Mathf.Sqrt(vel.x * vel.x + vel.z * vel.z);
			if (Mathf.Abs(num) <= 1f)
			{
				return targetY;
			}
			num = vel.z / num;
			float num2 = Mathf.Abs(Mathf.Acos(num)) * 57.29578f * Mathf.Sign(vel.x);
			float f = Mathf.DeltaAngle(Mathf.Repeat(targetY + 180f, 360f) - 180f, num2);
			if (Mathf.Abs(f) <= 110f)
			{
				return Mathf.LerpAngle(curY, num2 + (float)(90 * dfSteer), Time.deltaTime * 10f);
			}
			return Mathf.LerpAngle(curY, num2 + 180f + (float)(90 * dfSteer), Time.deltaTime * 10f);
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

		private IEnumerator buildTargetRigidbody()
		{
			yield return 0;
			while (carState.rigidbody == null)
			{
				yield return 0;
			}
			if (jRigidbody == null)
			{
				jRigidbody = new GameObject(view.name + "SYRD").AddComponent<Rigidbody>();
			}
			jRigidbody.isKinematic = true;
			jRigidbody.constraints = RigidbodyConstraints.FreezeAll;
			jRigidbody.detectCollisions = false;
			jRigidbody.position = carState.rigidbody.position;
			jRigidbody.transform.position = carState.transform.position;
			SpringJoint sj = jRigidbody.gameObject.AddComponent<SpringJoint>();
			sj.spring = 7000f;
			sj.maxDistance = 0.1f;
			sj.connectedBody = carState.rigidbody;
			joint = sj;
			Debug.LogError("Modify by Sbin: SyncMoveControllerClassic: dont SetConstraints");
			carState.rigidbody.drag = 9f;
			carState.rigidbody.mass = (float)carState.view.carModel.CarWeight * 0.8f;
			carState.view.SetSideWayFriction(50f);
			CarView carView = carState.view;
			carView.OnRelease = (Action)Delegate.Combine(carView.OnRelease, (Action)delegate
			{
				if ((bool)jRigidbody)
				{
					UnityEngine.Object.Destroy(jRigidbody.gameObject);
				}
			});
		}

		private void breakJoint()
		{
			if ((bool)joint)
			{
				joint.connectedBody = null;
			}
		}

		private void resetJoint()
		{
			if ((bool)joint && (bool)carState.rigidbody)
			{
				joint.GetComponent<Rigidbody>().position = carState.rigidbody.position;
				joint.transform.position = carState.rigidbody.position;
				joint.connectedBody = carState.rigidbody;
				Debug.LogWarning("Reset Joint " + Time.time);
			}
		}

		private void smoothMove()
		{
			if (UseAccel)
			{
				velocity += accel * Time.deltaTime;
			}
			velocity.y = 0f;
			if (!UseRigidbody)
			{
				if (controller != null && controller.enabled)
				{
					controller.SimpleMove(velocity);
					carState.rigidbody.position = carState.transform.position;
					carState.velocity = controller.velocity;
					carState.relativeVelocity = carState.transform.InverseTransformDirection(carState.velocity);
				}
			}
			else
			{
				vt = me.GetComponent<Rigidbody>().position + velocity * Time.deltaTime;
				vt.y = me.GetComponent<Rigidbody>().position.y;
				me.GetComponent<Rigidbody>().MovePosition(vt);
				carState.velocity = velocity;
				carState.relativeVelocity = carState.transform.InverseTransformDirection(carState.velocity);
			}
		}

		private void applyGravity()
		{
			if (!carState.rigidbody.isKinematic)
			{
				int wheelHitState = carState.WheelHitState;
				float num = -5f;
				float num2 = 5f;
				if (UseRigidbody)
				{
					num = -20f;
					num2 = 45f;
				}
				vt = gNormal;
				vt *= num * Time.deltaTime;
				carState.rigidbody.AddForce(vt, ForceMode.VelocityChange);
			}
		}

		private void nextTarget()
		{
			nextTarget(autoSetPosition: true);
		}

		private void nextTarget(bool autoSetPosition)
		{
			if (autoSetPosition && curTarget != null)
			{
				if (controller != null && controller.enabled)
				{
					controller.Move(curTarget.Position - me.position);
				}
				else
				{
					me.position = curTarget.Position;
				}
				me.rotation = curTarget.Rotation;
				carState.view.RotateAngleForCarModel(curTarget.CarModelRotation);
				Log("Set position ");
			}
			if (UseQueue && pathQueue.First != null)
			{
				curTarget = pathQueue.First.Value;
				pathQueue.RemoveFirst();
				losePackages += curTarget.Index - myIndex - 1;
				myIndex = curTarget.Index;
				vt = curTarget.EularAngle;
				vt.y = me.eulerAngles.y;
				me.eulerAngles = vt;
				emulateTarget();
			}
			else
			{
				clampVelocity(curTarget);
				curTarget = null;
			}
			if (curTarget != null)
			{
				curTarget.GetEfState(carState);
				curTarget.GetItems(carState);
				curTarget.GetScrapeValue(carState);
			}
		}

		private void emulateTarget()
		{
			if (me == null)
			{
				LogWarning("Error,me = null");
			}
			else
			{
				if (curTarget == null)
				{
					return;
				}
				vt = velocity;
				if (moveSpeed == 0f)
				{
					moveSpeed = curTarget.Speed;
				}
				float num = Vector3.Distance(curTarget.Position, me.position);
				float num2 = num / curTarget.Speed;
				if (num2 > 30f || num2 <= 0f)
				{
					nextTarget();
					Log("Exceed time ");
				}
				else
				{
					emuTarget = curTarget.Position + curTarget.Velocity * (num2 + 0.0005f * (float)(NetDelay + curTarget.Delay)) + 0.5f * curTarget.AccelVelocity * num2 * num2;
					num = Vector3.Distance(emuTarget, me.position);
					moveSpeed = num / num2;
					if (UseAccel)
					{
						vt3 = (emuTarget - me.position).normalized;
						float num3 = (num - velocity.magnitude * num2) / (0.5f * num2 * num2);
						accel = vt3 * num3;
						if (Vector3.Dot(vt3, vt.normalized) < 0.5f)
						{
							velocity = vt3 * velocity.magnitude;
							accel += (velocity - vt) / num2;
							velocity = vt;
						}
					}
					else
					{
						velocity = moveSpeed * (emuTarget - me.position).normalized;
					}
					if (UseEmulateTarget)
					{
						curTarget.Position += EmulateFactor * (emuTarget - curTarget.Position);
					}
				}
				velSyncDot = Vector3.Dot(velocity, vt);
			}
		}

		private void clampVelocity(MovementData target)
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
				moveSpeed = target.Speed;
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

		public void SendTransform()
		{
			vt = carState.TotalForces / carModel.CarWeight;
			MovementData movementData = new MovementData(carState, myIndex++, Time.time - lastSendTime, NetDelay);
			lastSendTime = Time.time;
			curTarget = movementData;
			netTimer[movementData.Index] = DateTime.Now.Ticks;
			if (OnSelfCarSendAction != null)
			{
				OnSelfCarSendAction(movementData.ToArray());
			}
			else
			{
				RaceCallback.SendRemote(movementData.ToArray());
			}
		}

		private void onSynTranslateCallback(CarState car, SpecialTriggerBase trigger, SpecialCallback cbType)
		{
		}

		private bool toggleTranslate(MovementData data)
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
							if (sc == SpecialCallback.Stop && curTarget == null)
							{
								velocity = carState.velocity;
							}
						});
						translateTrigger.Toggle(carState);
						curTarget = null;
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
			if (carState.CarPlayerType == PlayerType.PLAYER_OTHER)
			{
				MovementData movementData = new MovementData(bytes);
				if (!base.Active && !movementData.HasTranslateInfo)
				{
					return;
				}
				if (movementData.HasTranslateInfo && !hasTranslateInfo)
				{
					toggleTranslate(movementData);
				}
				hasTranslateInfo = movementData.HasTranslateInfo;
				if (hasTranslateInfo)
				{
					return;
				}
				float num = ((AsyncInterval > 0.15f) ? (0.5f * Mathf.Clamp(movementData.Delay + NetDelay, 0f, 1f) / 1000f) : 0f);
				vt2 = movementData.Position + movementData.Velocity * num;
				vt = vt2 - (UseRigidbody ? carState.rigidbody.position : me.position);
				curTarget = movementData;
				if (jumpDistance * jumpDistance < (movementData.Position - carState.rigidbody.position).sqrMagnitude)
				{
					if (UseRigidbody || JointMove)
					{
						carState.rigidbody.position = vt2;
						carState.rigidbody.rotation = curTarget.Rotation;
						resetJoint();
					}
					else
					{
						me.position = vt2;
						me.localRotation = curTarget.Rotation;
					}
					velocity = movementData.Velocity;
					carState.view.RotateAngleForCarModel(curTarget.CarModelRotation);
					curTarget = null;
					LogWarning("Jump");
				}
				else if (linearMove)
				{
					if (AsyncInterval > 0.15f)
					{
						num += AsyncInterval;
						vt += movementData.Velocity * num;
						velocity = vt / num;
					}
					else
					{
						velocity = (vt + movementData.Velocity * AsyncInterval * 4f) / (AsyncInterval * 4f);
					}
					emuPosition = velocity * num + (UseRigidbody ? carState.rigidbody.position : me.position);
					carState.velocity = velocity;
					carState.relativeVelocity = me.InverseTransformDirection(carState.velocity);
				}
				else
				{
					num = (duration = num + movementData.DeltaTime());
					vt = carState.rigidbody.position;
					p0.Set(vt.x, vt.z);
					vt2 = movementData.Position;
					p1.Set(vt2.x, vt2.z);
					vt = movementData.Position;
					p2.Set(vt.x, vt.z);
					vt = movementData.Position + movementData.Velocity * num;
					p3.Set(vt.x, vt.z);
					pStartTime = Time.time;
				}
			}
			else if (carState.CarPlayerType == PlayerType.PLAYER_SELF)
			{
				MovementData movementData2 = new MovementData(bytes);
				if (netTimer.ContainsKey(movementData2.Index))
				{
					long num2 = netTimer[movementData2.Index];
					netTimer.Remove(movementData2.Index);
					NetDelay = (DateTime.Now.Ticks - num2) / 10000;
					FPSShower.Messages["NetDelay"] = NetDelay + "ms";
				}
			}
		}

		public void AsyncMovement(MovementData data, bool forceSet)
		{
			if (carState.CarPlayerType == PlayerType.PLAYER_OTHER)
			{
				if (!base.Active && !data.HasTranslateInfo)
				{
					return;
				}
				if (data.HasTranslateInfo && !hasTranslateInfo)
				{
					toggleTranslate(data);
				}
				hasTranslateInfo = data.HasTranslateInfo;
				if (hasTranslateInfo)
				{
					return;
				}
				float num = ((AsyncInterval > 0.15f) ? (0.5f * Mathf.Clamp(data.Delay + NetDelay, 0f, 1f) / 1000f) : 0f);
				vt2 = data.Position + data.Velocity * num;
				vt = vt2 - (UseRigidbody ? carState.rigidbody.position : me.position);
				curTarget = data;
				if (jumpDistance * jumpDistance < (data.Position - carState.rigidbody.position).sqrMagnitude)
				{
					if (UseRigidbody || JointMove)
					{
						carState.rigidbody.position = vt2;
						carState.rigidbody.rotation = curTarget.Rotation;
						resetJoint();
					}
					else
					{
						me.position = vt2;
						me.localRotation = curTarget.Rotation;
					}
					carState.view.RotateAngleForCarModel(curTarget.CarModelRotation);
					velocity = data.Velocity;
					curTarget = null;
					LogWarning("Jump");
				}
				else if (linearMove)
				{
					if (AsyncInterval > 0.15f)
					{
						num += AsyncInterval;
						vt += data.Velocity * num;
						velocity = vt / num;
					}
					else
					{
						velocity = (vt + data.Velocity * AsyncInterval * 4f) / (AsyncInterval * 4f);
					}
					emuPosition = velocity * num + (UseRigidbody ? carState.rigidbody.position : me.position);
					carState.velocity = velocity;
					carState.relativeVelocity = me.InverseTransformDirection(carState.velocity);
				}
				else
				{
					num = (duration = num + data.DeltaTime());
					vt = carState.rigidbody.position;
					p0.Set(vt.x, vt.z);
					vt2 = data.Position;
					p1.Set(vt2.x, vt2.z);
					vt = data.Position;
					p2.Set(vt.x, vt.z);
					vt = data.Position + data.Velocity * num;
					p3.Set(vt.x, vt.z);
					pStartTime = Time.time;
				}
			}
			else if (carState.CarPlayerType == PlayerType.PLAYER_SELF && netTimer.ContainsKey(data.Index))
			{
				long num2 = netTimer[data.Index];
				netTimer.Remove(data.Index);
				NetDelay = (DateTime.Now.Ticks - num2) / 10000;
				FPSShower.Messages["NetDelay"] = NetDelay + "ms";
			}
		}

		private void onDrawGizmo()
		{
			if (carState.CarPlayerType == PlayerType.PLAYER_OTHER)
			{
				Gizmos.color = Color.red;
				Gizmos.DrawWireSphere(emuPosition, 0.5f);
				Debug.DrawLine(carState.rigidbody.position, carState.rigidbody.position + velocity.normalized * 5f, Color.yellow);
			}
		}

		private void syncUpdateCS()
		{
			if (curTarget != null)
			{
				if (JointMove)
				{
					carState.velocity = velocity - Vector3.Project(velocity, gNormal);
					vt = jRigidbody.position + carState.velocity * Time.deltaTime;
					jRigidbody.MovePosition(vt);
				}
				else if (UseRigidbody)
				{
					vt = carState.rigidbody.position + velocity * Time.deltaTime;
					if (carState.GroundHit != 0)
					{
						vt2.Set(0f, 1f, 0f);
						float num = Vector3.Dot(vt2, carState.GroundNormal);
						if (Mathf.Abs(num) < 0.95f)
						{
							num = 2.05f - num;
							float sqrMagnitude = velocity.sqrMagnitude;
							float num2 = Mathf.Lerp(-5f, -15f, Mathf.Clamp01(num * sqrMagnitude / 3000f));
							vt.y += num2 * Time.deltaTime;
						}
						else
						{
							vt.y += -1f * Time.deltaTime;
						}
					}
					carState.rigidbody.MovePosition(vt);
				}
				else
				{
					if (!(controller != null) || !controller.enabled)
					{
						return;
					}
					if (linearMove)
					{
						vt = velocity - Vector3.Project(velocity, gNormal);
						if (carState.rigidbody.useGravity)
						{
							me.localPosition = carState.rigidbody.position;
							controller.Move(vt * carState.view.TotalFixedDeltaTime);
							vt2 = carState.rigidbody.velocity;
							vt2.y = Mathf.Clamp(vt2.y, -5f, 5f);
							carState.rigidbody.velocity = vt2;
						}
						else
						{
							vt *= carState.view.TotalFixedDeltaTime;
							if (curTarget.IsGrounded())
							{
								if (controller.collisionFlags == CollisionFlags.None)
								{
									vt -= gNormal;
								}
								else if (carState.GroundHit == 0)
								{
									vt -= 20f * carState.view.TotalFixedDeltaTime * gNormal;
								}
								else
								{
									vt -= 6f * carState.view.TotalFixedDeltaTime * gNormal;
								}
							}
							else
							{
								vt -= 6f * carState.view.TotalFixedDeltaTime * gNormal;
							}
							controller.Move(vt);
							carState.rigidbody.MovePosition(me.localPosition);
						}
						carState.velocity = controller.velocity;
						carState.relativeVelocity = me.InverseTransformDirection(carState.velocity);
					}
					else
					{
						float num3 = (Time.time - pStartTime) / duration;
						if (num3 <= 1f)
						{
							tp = p0 * (1f - num3) * (1f - num3) * (1f - num3) + p1 * 3f * num3 * (1f - num3) * (1f - num3) + p2 * 3f * num3 * num3 * (1f - num3) + p3 * num3 * num3 * num3;
							vt = carState.rigidbody.position;
							me.localPosition = vt;
							vt2.Set(tp.x, vt.y - carState.view.TotalFixedDeltaTime, tp.y);
							controller.Move(vt2 - vt);
							velocity = controller.velocity;
							carState.velocity = velocity;
							carState.relativeVelocity = me.InverseTransformDirection(carState.velocity);
						}
						else
						{
							vt = velocity;
							vt.y = -10f;
							me.localPosition = carState.rigidbody.position;
							if (controller != null && controller.enabled)
							{
								controller.Move(vt * carState.view.TotalFixedDeltaTime);
							}
						}
					}
					if (ShowDebug)
					{
						Debug.DrawRay(carState.rigidbody.position, vt * AsyncInterval, Color.yellow);
					}
				}
			}
			else if (JointMove)
			{
				Log("^^^^^^^^^^");
			}
			else if (UseRigidbody)
			{
				vt = me.GetComponent<Rigidbody>().position + velocity * Time.deltaTime;
				vt.y = me.GetComponent<Rigidbody>().position.y;
				me.GetComponent<Rigidbody>().MovePosition(vt);
			}
			else
			{
				if (!(controller != null) || !controller.enabled || carState.ApplyingSpecialType == SpecialType.Translate)
				{
					return;
				}
				me.localPosition = carState.rigidbody.position;
				vt = velocity - Vector3.Project(velocity, gNormal);
				vt -= 20f * gNormal;
				controller.Move(vt * carState.view.TotalFixedDeltaTime);
				if (ShowDebug)
				{
					if (vt.sqrMagnitude < 0.1f)
					{
						Debug.DrawRay(carState.rigidbody.position, vt.normalized * AsyncInterval, Color.red);
					}
					else
					{
						Debug.DrawRay(carState.rigidbody.position, vt * AsyncInterval, Color.blue);
					}
				}
			}
		}

		private void flipCorrect()
		{
			if (controller != null && controller.enabled)
			{
				carState.rigidbody.MoveRotation(Quaternion.LookRotation(carState.rigidbody.rotation * vforward, gNormal));
				carState.eularAngle = carState.rigidbody.rotation.eulerAngles;
			}
		}

		private MovementData addQueue(MovementData data)
		{
			if (data.Index <= myIndex)
			{
				manulCast++;
				return null;
			}
			if (!UseQueue)
			{
				curTarget = data;
				if (myIndex > 0)
				{
					losePackages += curTarget.Index - myIndex - 1;
				}
				myIndex = curTarget.Index;
				emulateTarget();
				return data;
			}
			if (pathQueue.First == null)
			{
				pathQueue.AddFirst(data);
				return pathQueue.Last.Value;
			}
			LinkedListNode<MovementData> linkedListNode = pathQueue.First;
			foreach (MovementData item in pathQueue)
			{
				if (data.Index < item.Index)
				{
					pathQueue.AddBefore(linkedListNode, data);
					return pathQueue.Last.Value;
				}
				linkedListNode = linkedListNode.Next;
			}
			pathQueue.AddLast(data);
			return pathQueue.Last.Value;
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
			MovementData movementData = curTarget ?? buf2;
			if (movementData != null)
			{
				me.position = movementData.Position;
				me.rotation = movementData.Rotation;
				carState.view.ForceGround();
				movementData.GetEfState(carState);
				movementData.GetItems(carState);
				movementData.GetScrapeValue(carState);
				clampVelocity(movementData);
			}
		}
	}
}
