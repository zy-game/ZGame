using System;
using System.Collections.Generic;
using UnityEngine;

namespace CarLogic
{
	public class TranslateTrigger : SpecialTriggerBase
	{
		protected class RotateState
		{
			public RotType rotType;

			public RotState rotState;

			public float timeDelay;
		}

		protected enum RotType
		{
			Z,
			AntiZ,
			AntiX,
			Y,
			AntiY
		}

		protected enum RotState
		{
			None,
			Affecting,
			End
		}

		protected static RotType[,] RotTypeGroup = new RotType[4, 2]
		{
			{
				RotType.Z,
				RotType.Z
			},
			{
				RotType.Z,
				RotType.AntiY
			},
			{
				RotType.AntiZ,
				RotType.AntiZ
			},
			{
				RotType.AntiZ,
				RotType.AntiY
			}
		};

		protected Rigidbody rigidbody;

		protected CharacterController controller;

		protected TranslatePath path;

		protected float duration = 3f;

		protected float pastTime;

		protected TranslatePoint tp;

		protected float steerAngle;

		protected float startSpeed;

		protected Vector3 startVelocity = Vector3.zero;

		protected Ray tRay;

		protected int roadMask = LayerMask.GetMask("Road");

		protected RaycastHit rayHit;

		protected Vector3 rightDir = Vector3.zero;

		protected const float minEndSpeed = 50f;

		private bool grounded;

		protected AnimationCurve curve;

		protected Collider endCollider;

		protected BoolFlag paused = new BoolFlag();

		protected HashSet<object> affectingItem = new HashSet<object>();

		protected static Vector3 vup = Vector3.up;

		protected float smoothTime = 0.2f;

		protected float qteScale = 1f;

		protected float qteRotSpeed = 360f / RaceConfig.TransQTECircleDur;

		protected int useRotGroup;

		protected RotateState norRotState = new RotateState();

		protected int qteLevel;

		protected float qteRotTime;

		protected RotType qteRotType;

		protected RotState qteRotState;

		protected Transform qteRoot;

		protected Transform qteEffect;

		protected float dampFactor = 0.08f;

		private float camDamper = 0.985f;

		private int dampCount;

		public override SpecialType Type => SpecialType.Translate;

		public override float Duration
		{
			get
			{
				return duration;
			}
			set
			{
				duration = value;
			}
		}

		public bool Grounded => grounded;

		public int QTELevel => qteLevel;

		public TranslateTrigger(TranslatePath path, float duration, Collider ender, bool grounded = false, AnimationCurve speedCurve = null)
		{
			this.path = path;
			this.duration = duration;
			this.grounded = grounded;
			curve = speedCurve;
			endCollider = ender;
			for (int i = 0; i < RaceConfig.TRANSLATE_QTE_LEVELS.Length; i++)
			{
				qteLevel = i;
				if (duration < RaceConfig.TRANSLATE_QTE_LEVELS[i])
				{
					break;
				}
			}
		}

		public override void Toggle(CarState state)
		{
			base.Toggle(state);
			if (state != null && state.rigidbody != null)
			{
				useRotGroup = UnityEngine.Random.Range(0, RotTypeGroup.GetLength(0));
				state.TransQTEok = 0;
				buildPuller(state);
				tp = new TranslatePoint();
				pastTime = 0f;
				CarCallBack callBacks = target.CallBacks;
				callBacks.OnAffectedByItem = (Action<RaceItemId, CarState, ItemCallbackType, object>)Delegate.Remove(callBacks.OnAffectedByItem, new Action<RaceItemId, CarState, ItemCallbackType, object>(onAffectByItem));
				CarCallBack callBacks2 = target.CallBacks;
				callBacks2.OnAffectedByItem = (Action<RaceItemId, CarState, ItemCallbackType, object>)Delegate.Combine(callBacks2.OnAffectedByItem, new Action<RaceItemId, CarState, ItemCallbackType, object>(onAffectByItem));
				if (!grounded)
				{
					CarCallBack callBacks3 = target.CallBacks;
					callBacks3.AffectChecker = (ModifyAction<RaceItemId, bool>)Delegate.Remove(callBacks3.AffectChecker, new ModifyAction<RaceItemId, bool>(itemChecker));
					CarCallBack callBacks4 = target.CallBacks;
					callBacks4.AffectChecker = (ModifyAction<RaceItemId, bool>)Delegate.Combine(callBacks4.AffectChecker, new ModifyAction<RaceItemId, bool>(itemChecker));
				}
				SetCollidersActive<Collider, BoxCollider>(target.view.BodyColliders, val: false);
				if (!grounded)
				{
					SetCollidersActive<Transform, WheelCollider>(target.view.frontWheels, val: false);
					SetCollidersActive<Transform, WheelCollider>(target.view.rearWheels, val: false);
				}
				target.view.PlayOnJumping(duration);
			}
		}

		protected virtual void buildPuller(CarState state)
		{
			if (state == null || state.rigidbody == null)
			{
				return;
			}
			if (CameraController.Current != null && CameraController.Current.ViewTarget == state)
			{
				CameraController.Current.UseVelocityDir = true;
			}
			if (state.CarPlayerType == PlayerType.PLAYER_SELF || state.CarPlayerType == PlayerType.PLAYER_AUTO_AI)
			{
				state.view.DriftEnable = false;
				state.view.Resetable = false;
				CarView view = state.view;
				view.OnFixedupdate = (Action)Delegate.Remove(view.OnFixedupdate, new Action(selfUpdate));
				CarView view2 = state.view;
				view2.OnFixedupdate = (Action)Delegate.Combine(view2.OnFixedupdate, new Action(selfUpdate));
				CarView view3 = state.view;
				view3.OnTriggerBegin = (Action<Collider>)Delegate.Remove(view3.OnTriggerBegin, new Action<Collider>(onCollideEnder));
				CarView view4 = state.view;
				view4.OnTriggerBegin = (Action<Collider>)Delegate.Combine(view4.OnTriggerBegin, new Action<Collider>(onCollideEnder));
				startVelocity = state.rigidbody.velocity;
				state.rigidbody.velocity = Vector3.zero;
				state.rigidbody.angularVelocity = Vector3.zero;
				RigidbodyTool.SetConstraints(state.view, RigidbodyConstraints.FreezeAll);
				state.view.DisableSidewayFriction();
				if (grounded)
				{
					state.CurDriftState.Stage = DriftStage.REMAIN;
				}
				startSpeed = state.velocity.magnitude;
				controller = state.transform.GetComponent<CharacterController>();
				if (!controller)
				{
					controller = state.view.AddCharacterController();
				}
				controller.enabled = false;
				controller.enabled = true;
			}
			else if (state.CarPlayerType == PlayerType.PLAYER_OTHER)
			{
				controller = state.transform.GetComponent<CharacterController>();
				if (!controller)
				{
					controller = state.view.AddCharacterController();
				}
				controller.enabled = true;
				CarView view5 = target.view;
				view5.OnFixedupdate = (Action)Delegate.Remove(view5.OnFixedupdate, new Action(synUpdate));
				CarView view6 = target.view;
				view6.OnFixedupdate = (Action)Delegate.Combine(view6.OnFixedupdate, new Action(synUpdate));
				CarView view7 = state.view;
				view7.OnTriggerBegin = (Action<Collider>)Delegate.Remove(view7.OnTriggerBegin, new Action<Collider>(onCollideEnder));
				CarView view8 = state.view;
				view8.OnTriggerBegin = (Action<Collider>)Delegate.Combine(view8.OnTriggerBegin, new Action<Collider>(onCollideEnder));
			}
			else if (state.CarPlayerType == PlayerType.PALYER_AI)
			{
				CarView view9 = target.view;
				view9.OnFixedupdate = (Action)Delegate.Remove(view9.OnFixedupdate, new Action(aiUpdate));
				CarView view10 = target.view;
				view10.OnFixedupdate = (Action)Delegate.Combine(view10.OnFixedupdate, new Action(aiUpdate));
			}
		}

		protected virtual void onCollideEnder(Collider c)
		{
			if ((object)c == endCollider)
			{
				Stop();
			}
		}

		protected virtual void onAffectByItem(RaceItemId itemid, CarState state, ItemCallbackType icb, object o)
		{
			if (!needPause(itemid))
			{
				return;
			}
			switch (icb)
			{
				case ItemCallbackType.AFFECT:
					++paused;
					affectingItem.Add(o);
					break;
				case ItemCallbackType.BREAK:
					if (affectingItem.Contains(o))
					{
						--paused;
						affectingItem.Remove(o);
					}
					break;
			}
		}

		protected virtual void itemChecker(RaceItemId id, ref bool usable)
		{
			usable &= id != RaceItemId.ROCKET && id != RaceItemId.MINE && id != RaceItemId.WATER_FLY && id != RaceItemId.WATER_BOMB && id != RaceItemId.CHEAT_BOX && id != RaceItemId.BANANA;
		}

		protected virtual bool needPause(RaceItemId item)
		{
			bool result = false;
			switch (item)
			{
				case RaceItemId.CHEAT_BOX:
					result = true;
					break;
				case RaceItemId.MINE:
					result = true;
					break;
				case RaceItemId.ROCKET:
					result = true;
					break;
				case RaceItemId.WATER_BOMB:
					result = true;
					break;
				case RaceItemId.WATER_FLY:
					result = true;
					break;
			}
			return result;
		}

		protected void checkGrounded()
		{
			if (target == null || null == target.view)
			{
				return;
			}
			if (null != endCollider)
			{
				tRay.origin = target.view.transform.position;
				tRay.direction = -endCollider.transform.up;
				if (Physics.Raycast(tRay, out rayHit, 2f, roadMask))
				{
					target.view.transform.forward = Vector3.Cross(rayHit.normal, -rightDir);
				}
			}
			if (target.GroundHit > 0)
			{
				target.rigidbody.velocity = target.transform.forward * startVelocity.magnitude * 1.5f;
				CarView view = target.view;
				view.OnFixedupdate = (Action)Delegate.Remove(view.OnFixedupdate, new Action(checkGrounded));
			}
		}

		protected virtual void selfUpdate()
		{
			if (target == null || target.view == null || path == null)
			{
				Stop();
			}
			else if (pastTime > duration)
			{
				Stop();
			}
			else
			{
				if ((bool)paused)
				{
					return;
				}
				calSteerAngle();
				float num = Mathf.Clamp01(pastTime / duration);
				if (curve != null)
				{
					num = curve.Evaluate(num);
				}
				path.Interp(num, tp);
				Vector3 vector = tp.Rotation * vup;
				CheckQteClick();
				tp.Rotation *= Quaternion.AngleAxis(steerAngle, Vector3.up);
				dampFactor = Mathf.Lerp(0.01f, 0.08f, Mathf.Clamp01(pastTime / Mathf.Min(0.8f, duration / 3f)));
				tp.Rotation = Quaternion.Lerp(target.rigidbody.rotation, tp.Rotation, dampFactor);
				tp.Position = Vector3.Lerp(target.rigidbody.position, tp.Position, dampFactor);
				if (grounded)
				{
					tp.Position -= vector;
				}
				controller.Move(tp.Position - target.rigidbody.position);
				target.rigidbody.position = target.transform.position;
				target.rigidbody.velocity = controller.velocity;
				target.view.Controller.calculateRelativeVelocity();
				target.view.Controller.setWheelState(target);
				target.view.Controller.calGroundNormal();
				target.velocity = controller.velocity;
				target.SpeedRatio = target.velocity.magnitude / (float)target.view.carModel.MaxSpeeds[0];
				if (grounded)
				{
					vector = ((target.GroundHit == 0) ? target.view.GetGNormalByNode(target.rigidbody.position) : target.GroundNormal);
					tp.Rotation = Quaternion.LookRotation(Vector3.Cross(vector, tp.Rotation * Vector3.left), vector);
				}
				target.rigidbody.rotation = tp.Rotation;
				float num2 = Time.deltaTime * qteScale;
				int lerpAIndex = path.GetLerpAIndex(num);
				if (lerpAIndex >= 0 && lerpAIndex < path.PointList.Count)
				{
					TranslatePoint translatePoint = path.PointList[lerpAIndex];
					num2 /= translatePoint.Weight;
				}
				pastTime += num2;
				if (null != CameraController.Current && target.CarPlayerType == PlayerType.PLAYER_SELF)
				{
					if (dampCount < 40 && CameraController.Current.ViewTarget == target)
					{
						CameraController.Current.PosDamping *= camDamper;
						dampCount++;
						CameraController.Current.PosDamping *= camDamper;
						dampCount++;
					}
					CameraController.Current.transform.position += tp.CameraOffset;
					CameraController.Current.transform.LookAt(target.rigidbody.position);
				}
			}
		}

		private void CheckQteClick()
		{
			if (!grounded && qteRotState == RotState.None && target.TransQTEok == 1)
			{
				qteRotState = RotState.Affecting;
				qteRotTime = 0f;
				qteScale = 1.2f;
				target.view.StartSmallGas();
				if (qteRoot == null && path != null && path.QteAnimations != null)
				{
					setupQteRoot();
				}
			}
		}

		private void GetQteAnimIndex()
		{
			if (target.TransQteAnimIndex < 0 || target.TransQteAnimIndex >= path.QteAnimations.Length)
			{
				target.TransQteAnimIndex = (int)(Time.time * 1000f) % path.QteAnimations.Length;
			}
		}

		private void setupQteRoot()
		{
			int childCount = target.transform.childCount;
			Transform[] array = new Transform[childCount];
			for (int i = 0; i < childCount; i++)
			{
				array[i] = target.transform.GetChild(i);
			}
			qteRoot = new GameObject("qteRoot").transform;
			qteRoot.parent = target.transform;
			qteRoot.localRotation = Quaternion.identity;
			qteRoot.localScale = Vector3.one;
			if (target.view.CarRenders != null && target.view.CarRenders.Length != 0 && target.view.CarRenders[0] != null)
			{
				qteRoot.position = target.view.CarRenders[0].bounds.center;
			}
			else
			{
				qteRoot.localPosition = new Vector3(0f, 0.2f, 0f);
			}
			if (path != null && path.QteAnimations != null && path.QteAnimations.Length != 0)
			{
				Animation animation = qteRoot.gameObject.AddComponent<Animation>();
				GetQteAnimIndex();
				int transQteAnimIndex = target.TransQteAnimIndex;
				animation.AddClip(path.QteAnimations[transQteAnimIndex], path.QteAnimations[transQteAnimIndex].name);
				animation.Play(path.QteAnimations[transQteAnimIndex].name);
			}
			for (int j = 0; j < array.Length; j++)
			{
				array[j].parent = qteRoot;
			}
			Singleton<ResourceOffer>.Instance.Load(RaceConfig.TranslateQteEffect, delegate(UnityEngine.Object o)
			{
				if (o is GameObject)
				{
					qteEffect = (UnityEngine.Object.Instantiate(o) as GameObject).transform;
					qteEffect.parent = target.transform;
					if (target.view.CarRenders != null && target.view.CarRenders.Length != 0 && target.view.CarRenders[0] != null)
					{
						qteEffect.position = target.view.CarRenders[0].bounds.center;
					}
					else
					{
						qteEffect.localPosition = new Vector3(0f, 0.2f, 0f);
					}
				}
			});
		}

		protected virtual void calSteerAngle()
		{
			if (target.Steer != 0f && qteRotState != RotState.Affecting)
			{
				float num = target.Steer * (float)target.view.carModel.MaximumTurn * target.view.TotalFixedDeltaTime;
				steerAngle = Mathf.Clamp(steerAngle + num, -90f, 90f);
			}
		}

		private void SynCheckQteClick()
		{
			if (!grounded && qteRotState == RotState.None && target.TransQTEok == 1)
			{
				qteRotState = RotState.Affecting;
				qteRotTime = 0f;
				qteScale = 1.2f;
				if (qteRoot == null && path != null && path.QteAnimations != null)
				{
					setupQteRoot();
				}
			}
		}

		protected virtual void synUpdate()
		{
			if (target == null || target.view == null || path == null || controller == null)
			{
				Stop();
			}
			else if (!paused)
			{
				calSteerAngle();
				float num = Mathf.Clamp01(pastTime / duration);
				if (curve != null)
				{
					num = curve.Evaluate(num);
				}
				path.Interp(num, tp);
				Vector3 vector = tp.Rotation * vup;
				SynCheckQteClick();
				tp.Rotation *= Quaternion.AngleAxis(steerAngle, vector);
				tp.Rotation = Quaternion.Lerp(target.rigidbody.rotation, tp.Rotation, 0.08f);
				tp.Position = Vector3.Lerp(target.rigidbody.position, tp.Position, 0.08f);
				if (grounded)
				{
					tp.Position -= vector;
				}
				controller.Move(tp.Position - target.rigidbody.position);
				target.rigidbody.position = target.transform.position;
				if (SyncMoveController.OpenNewSyncMove == false)
				{
					target.view.Controller.calculateRelativeVelocity();
				}
				if (!grounded)
				{
					target.view.Controller.setWheelState(target);
				}
				target.view.Controller.calGroundNormal();
				target.velocity = controller.velocity;
				if (grounded)
				{
					vector = ((target.GroundHit == 0) ? target.view.GetGNormalByNode(target.rigidbody.position) : target.GroundNormal);
					tp.Rotation = Quaternion.LookRotation(Vector3.Cross(vector, tp.Rotation * Vector3.left), vector);
				}
				target.rigidbody.rotation = tp.Rotation;
				float num2 = Time.deltaTime * qteScale;
				int lerpAIndex = path.GetLerpAIndex(num);
				if (lerpAIndex >= 0 && lerpAIndex < path.PointList.Count)
				{
					TranslatePoint translatePoint = path.PointList[lerpAIndex];
					num2 /= translatePoint.Weight;
				}
				pastTime += num2;
				if (pastTime > duration)
				{
					Stop();
				}
			}
		}

		protected virtual void aiUpdate()
		{
			if (target == null || target.view == null || path == null)
			{
				Stop();
			}
			else if (!paused)
			{
				CheckQteClick();
				float t = pastTime / duration;
				float num = Time.deltaTime;
				int lerpAIndex = path.GetLerpAIndex(t);
				if (lerpAIndex >= 0 && lerpAIndex < path.PointList.Count)
				{
					TranslatePoint translatePoint = path.PointList[lerpAIndex];
					num /= translatePoint.Weight;
				}
				pastTime += num;
				if (pastTime > duration)
				{
					Stop();
				}
			}
		}

		private void SetCollidersActive<T1, T2>(T1[] colliders, bool val) where T1 : Component where T2 : Collider
		{
			if (colliders == null || colliders.Length == 0)
			{
				return;
			}
			for (int i = 0; i < colliders.Length; i++)
			{
				if (null != colliders[i])
				{
					T2 component = colliders[i].GetComponent<T2>();
					if (null != component)
					{
						component.enabled = val;
					}
				}
			}
		}

		private void DestroyQteRoot()
		{
			if (qteRoot != null)
			{
				qteRoot.localRotation = Quaternion.identity;
				int childCount = qteRoot.childCount;
				for (int num = childCount - 1; num >= 0; num--)
				{
					qteRoot.GetChild(num).parent = target.transform;
				}
				UnityEngine.Object.Destroy(qteRoot.gameObject);
			}
			if (qteEffect != null)
			{
				UnityEngine.Object.Destroy(qteEffect.gameObject);
				qteEffect = null;
			}
		}

		public override void Stop()
		{
			base.Stop();
			qteRotState = RotState.None;
			if (target == null || !(target.view != null))
			{
				return;
			}
			if (CameraController.Current != null && CameraController.Current.ViewTarget == target)
			{
				CameraController.Current.UseVelocityDir = false;
			}
			SetCollidersActive<Collider, BoxCollider>(target.view.BodyColliders, val: true);
			if (!grounded)
			{
				SetCollidersActive<Transform, WheelCollider>(target.view.frontWheels, val: true);
				SetCollidersActive<Transform, WheelCollider>(target.view.rearWheels, val: true);
			}
			target.TransQteAnimIndex = -1;
			CarCallBack callBacks = target.CallBacks;
			callBacks.AffectChecker = (ModifyAction<RaceItemId, bool>)Delegate.Remove(callBacks.AffectChecker, new ModifyAction<RaceItemId, bool>(itemChecker));
			CarCallBack callBacks2 = target.CallBacks;
			callBacks2.OnAffectedByItem = (Action<RaceItemId, CarState, ItemCallbackType, object>)Delegate.Remove(callBacks2.OnAffectedByItem, new Action<RaceItemId, CarState, ItemCallbackType, object>(onAffectByItem));
			if (target.CarPlayerType == PlayerType.PLAYER_SELF || target.CarPlayerType == PlayerType.PLAYER_AUTO_AI)
			{
				if (rigidbody != null)
				{
					UnityEngine.Object.Destroy(rigidbody.gameObject);
				}
				if (CameraController.Current != null && CameraController.Current.ViewTarget == target)
				{
					Action dac = null;
					dac = delegate
					{
						if (dampCount <= 0 || null == CameraController.Current)
						{
							CarView view12 = target.view;
							view12.OnFixedupdate = (Action)Delegate.Remove(view12.OnFixedupdate, dac);
						}
						else
						{
							CameraController.Current.PosDamping /= camDamper;
							dampCount--;
						}
					};
					CarView view = target.view;
					view.OnFixedupdate = (Action)Delegate.Combine(view.OnFixedupdate, dac);
				}
				if (!(target.view != null))
				{
					return;
				}
				target.view.DriftEnable = true;
				target.view.Resetable = true;
				rightDir = target.view.transform.right;
				CarView view2 = target.view;
				view2.OnFixedupdate = (Action)Delegate.Remove(view2.OnFixedupdate, new Action(selfUpdate));
				CarView view3 = target.view;
				view3.OnFixedupdate = (Action)Delegate.Remove(view3.OnFixedupdate, new Action(checkGrounded));
				CarView view4 = target.view;
				view4.OnFixedupdate = (Action)Delegate.Combine(view4.OnFixedupdate, new Action(checkGrounded));
				CarView view5 = target.view;
				view5.OnTriggerBegin = (Action<Collider>)Delegate.Remove(view5.OnTriggerBegin, new Action<Collider>(onCollideEnder));
				RigidbodyTool.SetConstraints(target.view, RigidbodyConstraints.None);
				target.CurDriftState.Stage = DriftStage.NONE;
				target.TransQTEok = 0;
				if ((bool)controller && controller.enabled)
				{
					Vector3 velocity = controller.velocity;
					target.rigidbody.velocity = velocity;
					controller.enabled = false;
					target.view.Controller.fixedUpdate();
				}
				DestroyQteRoot();
				if (!target.view.EnableSidewayFriction() || !(Mathf.Abs(steerAngle) > 10f))
				{
					return;
				}
				float st = Time.time;
				float dua = Mathf.Abs(steerAngle) / 60f * 0.6f;
				Action ac = null;
				ac = delegate
				{
					float num = Time.time - st;
					num /= dua;
					if (num > 1f)
					{
						CarView view10 = target.view;
						view10.OnFixedupdate = (Action)Delegate.Remove(view10.OnFixedupdate, ac);
					}
					else if (target.SidewayFrictionFlag != 0)
					{
						CarView view11 = target.view;
						view11.OnFixedupdate = (Action)Delegate.Remove(view11.OnFixedupdate, ac);
					}
					else
					{
						target.view.SetSideWayFriction(Mathf.Lerp(0f, target.view.carModel.SideWayFriction, num * num * num));
					}
				};
				target.view.SetSideWayFriction(0f);
				CarView view6 = target.view;
				view6.OnFixedupdate = (Action)Delegate.Combine(view6.OnFixedupdate, ac);
			}
			else if (target.CarPlayerType == PlayerType.PLAYER_OTHER)
			{
				CarView view7 = target.view;
				view7.OnFixedupdate = (Action)Delegate.Remove(view7.OnFixedupdate, new Action(synUpdate));
				CarView view8 = target.view;
				view8.OnTriggerBegin = (Action<Collider>)Delegate.Remove(view8.OnTriggerBegin, new Action<Collider>(onCollideEnder));
				DestroyQteRoot();
				target.TransQTEok = 0;
				controller.enabled = false;
			}
			else if (target.CarPlayerType == PlayerType.PALYER_AI)
			{
				CarView view9 = target.view;
				view9.OnFixedupdate = (Action)Delegate.Remove(view9.OnFixedupdate, new Action(aiUpdate));
			}
		}
	}
}
