using System;
using UnityEngine;

namespace CarLogic
{
	public class BananaTrigger : PassiveTrigger
	{
		private float flipSpeed = 25f;

		private const float roundTimes = 3f;

		private float rotateSpeed = 540f;

		private float flipTime = 2f;

		private Vector3 dir;

		private Vector3 odir;

		private Vector3 up = new Vector3(0f, 1f, 0f);

		private Vector3 vt;

		private float startTime;

		private CharacterController controller;

		private EffectToggleBase effectObject;

		private Plane plane;

		private GameObject soundObject;

		public override RaceItemId ItemId => RaceItemId.BANANA;

		public BananaTrigger(ushort id, long playerId = 0L)
			: base(id, playerId)
		{
			flipTime = RaceItemFactory.GetDelayDuration(ItemId, isTrigger: true);
			rotateSpeed = 1080f / flipTime;
			plane = default(Plane);
		}

		public override void ApplyEffect(ItemParams ps)
		{
			base.ApplyEffect(ps);
			Release();
			if (carState == null)
			{
				Stop();
				return;
			}
			bool r = true;
			if (carState.CallBacks.AffectChecker != null)
			{
				carState.CallBacks.AffectChecker(ItemId, ref r);
			}
			if (carState != null && carState.CallBacks.OnAffectedByItem != null)
			{
				carState.CallBacks.OnAffectedByItem(ItemId, carState, ItemCallbackType.CHECK, this);
			}
			if (!r)
			{
				Stop();
				return;
			}
			flipSpeed = carState.velocity.magnitude * 0.5f;
			CarCallBack callBacks = carState.CallBacks;
			callBacks.AffectChecker = (ModifyAction<RaceItemId, bool>)Delegate.Combine(callBacks.AffectChecker, new ModifyAction<RaceItemId, bool>(affectChecker));
			startTime = Time.time;
			if (carState.velocity.sqrMagnitude < 1f)
			{
				dir = carState.transform.forward;
			}
			else
			{
				dir = carState.velocity.normalized;
			}
			odir = dir;
			if (carState.CarPlayerType == PlayerType.PLAYER_SELF)
			{
				playerMoveStart();
			}
			else
			{
				emulateMoveStart();
			}
			Singleton<ResourceOffer>.Instance.Load(RaceConfig.ItemBananaEffect, delegate(UnityEngine.Object o)
			{
				if (!(carState.view == null) && !(null == o) && status != 0)
				{
					GameObject gameObject = UnityEngine.Object.Instantiate(o) as GameObject;
					effectObject = gameObject.AddComponent<EffectToggleBase>();
					effectObject.Init();
					Transform transform = gameObject.transform;
					transform.parent = carState.transform;
					transform.localPosition = Vector3.zero;
					transform.localRotation = Quaternion.identity;
					effectObject.Active = true;
				}
			});
			if (carState != null && carState.CallBacks.OnAffectedByItem != null)
			{
				carState.CallBacks.OnAffectedByItem(ItemId, carState, ItemCallbackType.AFFECT, this);
			}
			if (RaceAudioManager.ActiveInstance != null)
			{
				soundObject = new GameObject("banana");
				soundObject.transform.parent = carState.transform;
				soundObject.transform.localPosition = Vector3.zero;
				AudioSource audioSource = soundObject.AddComponent<AudioSource>();
				audioSource.clip = RaceAudioManager.ActiveInstance.Sound_xiangjiaopi;
				audioSource.loop = false;
				audioSource.Play();
			}
			if (carState.CallBacks.OnFreezeMoveEffect != null)
			{
				Debug.LogWarning("OnFreezeMoveEffect");
				carState.CallBacks.OnFreezeMoveEffect(ItemId, RaceItemFactory.GetDelayDuration(ItemId, isTrigger: true));
			}
		}

		private void checker(ref bool enable)
		{
			enable = false;
		}

		private void affectChecker(RaceItemId id, ref bool usable)
		{
			if (usable && (id == RaceItemId.GAS || id == RaceItemId.GROUP_GAS))
			{
				usable = false;
			}
		}

		private void fixedupdate()
		{
			if (Time.time - startTime > flipTime || carState.CollisionDirection != 0)
			{
				Stop();
			}
			else if (carState.CarPlayerType == PlayerType.PLAYER_SELF)
			{
				carState.velocity = dir * flipSpeed;
				carState.TotalForces.Set(0f, 0f, 0f);
				vt.Set(0f, 1f, 0f);
				Quaternion quaternion = Quaternion.AngleAxis(rotateSpeed * Time.deltaTime, vt);
				carState.rigidbody.rotation *= quaternion;
				vt = carState.LastNormal;
				dir = updateDir(dir, vt, carState.rigidbody.position);
				carState.rigidbody.velocity = dir * flipSpeed - vt * Mathf.Lerp(18f, 2f, Mathf.Clamp01(carState.view.GetGroundedPercent()));
			}
		}

		private void update()
		{
			if (Time.time - startTime > flipTime || carState.CollisionDirection != 0)
			{
				Stop();
			}
			else if (controller != null && controller.enabled)
			{
				if (carState.ApplyingSpecialType != SpecialType.Translate)
				{
					vt = carState.LastNormal;
					dir = updateDir(dir, vt, carState.rigidbody.position);
					vt = (dir * flipSpeed - vt * 5f) * Time.deltaTime;
					controller.Move(vt);
					carState.rigidbody.position = carState.transform.position;
				}
				carState.transform.Rotate(up, rotateSpeed * Time.deltaTime, Space.Self);
				carState.rigidbody.MoveRotation(carState.transform.rotation);
			}
			else
			{
				if (carState.ApplyingSpecialType != SpecialType.Translate)
				{
					vt = carState.LastNormal;
					dir = updateDir(dir, vt, carState.rigidbody.position);
					vt = (dir * flipSpeed - vt * Mathf.Lerp(14f, 2f, Mathf.Clamp01(carState.view.GetGroundedPercent()))) * Time.deltaTime;
					carState.rigidbody.MovePosition(carState.rigidbody.position + vt);
				}
				vt.Set(0f, 1f, 0f);
				Quaternion quaternion = Quaternion.AngleAxis(rotateSpeed * Time.deltaTime, vt);
				carState.rigidbody.rotation *= quaternion;
			}
		}

		private void setState(CarState state)
		{
			state.Throttle = 0.01f;
			state.Steer = 0f;
		}

		private void playerMoveStart()
		{
			CarCallBack callBacks = carState.CallBacks;
			callBacks.OnApplyForce = (Action)Delegate.Combine(callBacks.OnApplyForce, new Action(fixedupdate));
			CarCallBack callBacks2 = carState.CallBacks;
			callBacks2.OnSetState = (Action<CarState>)Delegate.Combine(callBacks2.OnSetState, new Action<CarState>(setState));
			CarCallBack callBacks3 = carState.CallBacks;
			callBacks3.InverseChecker = (ModifyAction<bool>)Delegate.Combine(callBacks3.InverseChecker, new ModifyAction<bool>(checker));
			CarCallBack callBacks4 = carState.CallBacks;
			callBacks4.ResetChecker = (ModifyAction<bool>)Delegate.Combine(callBacks4.ResetChecker, new ModifyAction<bool>(checker));
			if ((bool)carState.rigidbody && !carState.rigidbody.isKinematic)
			{
				carState.rigidbody.velocity = flipSpeed * dir;
				carState.view.DisableSidewayFriction();
			}
			carState.view.DriftEnable = false;
			if (CameraController.Current != null && CameraController.Current.ViewTarget == carState)
			{
				CameraController.Current.UseVelocityDir = true;
			}
			for (int i = 0; i < carState.Wheels.Length; i++)
			{
				Wheel wheel = carState.Wheels[i];
				JointSpring suspensionSpring = wheel.collider.suspensionSpring;
				suspensionSpring.spring *= 12f;
			}
		}

		private void emulateMoveStart()
		{
			controller = carState.view.AddCharacterController();
			controller.enabled = true;
			CarView view = carState.view;
			view.OnFixedupdate = (Action)Delegate.Combine(view.OnFixedupdate, new Action(update));
			if (carState.CarPlayerType == PlayerType.PLAYER_OTHER)
			{
				carState.view.SyncController.Active = false;
			}
			else if (carState.CarPlayerType == PlayerType.PALYER_AI)
			{
				carState.view.Ai.Paused = true;
			}
			if (CameraController.Current != null && CameraController.Current.ViewTarget == carState)
			{
				CameraController.Current.UseVelocityDir = true;
			}
		}

		private Vector3 updateDir(Vector3 oldDir, Vector3 normal, Vector3 pos)
		{
			plane.SetNormalAndPosition(normal, pos);
			Ray ray = new Ray(pos + normal, oldDir - normal);
			float enter = 0f;
			if (plane.Raycast(ray, out enter))
			{
				oldDir = ray.GetPoint(enter) - pos;
			}
			return oldDir.normalized;
		}

		public override void Release()
		{
			base.Release();
		}

		public override void Stop()
		{
			base.Stop();
			if (effectObject != null)
			{
				effectObject.Active = false;
				RaceCallback.View.CallDelay(delegate
				{
					if (effectObject != null)
					{
						UnityEngine.Object.Destroy(effectObject.gameObject);
					}
				}, 1f);
			}
			if (carState != null)
			{
				CarCallBack callBacks = carState.CallBacks;
				callBacks.AffectChecker = (ModifyAction<RaceItemId, bool>)Delegate.Remove(callBacks.AffectChecker, new ModifyAction<RaceItemId, bool>(affectChecker));
			}
			if (CameraController.Current != null && CameraController.Current.ViewTarget == carState)
			{
				CameraController.Current.ResetCurRotDamp();
				CameraController.Current.UseVelocityDir = false;
			}
			if (carState != null && carState.CarPlayerType == PlayerType.PLAYER_SELF)
			{
				CarCallBack callBacks2 = carState.CallBacks;
				callBacks2.OnSetState = (Action<CarState>)Delegate.Remove(callBacks2.OnSetState, new Action<CarState>(setState));
				CarCallBack callBacks3 = carState.CallBacks;
				callBacks3.OnApplyForce = (Action)Delegate.Remove(callBacks3.OnApplyForce, new Action(fixedupdate));
				CarCallBack callBacks4 = carState.CallBacks;
				callBacks4.InverseChecker = (ModifyAction<bool>)Delegate.Remove(callBacks4.InverseChecker, new ModifyAction<bool>(checker));
				CarCallBack callBacks5 = carState.CallBacks;
				callBacks5.ResetChecker = (ModifyAction<bool>)Delegate.Remove(callBacks5.ResetChecker, new ModifyAction<bool>(checker));
				carState.view.SyncController.ResetTimeLeft();
				if ((bool)carState.rigidbody && !carState.rigidbody.isKinematic)
				{
					carState.view.EnableSidewayFriction();
				}
				for (int i = 0; i < carState.Wheels.Length; i++)
				{
					Wheel wheel = carState.Wheels[i];
					JointSpring suspensionSpring = wheel.collider.suspensionSpring;
					suspensionSpring.spring /= 12f;
				}
				if (carState.ApplyingSpecialType != SpecialType.Translate)
				{
					carState.view.DriftEnable = true;
				}
			}
			else
			{
				CarView view = carState.view;
				view.OnFixedupdate = (Action)Delegate.Remove(view.OnFixedupdate, new Action(update));
				if (carState.CarPlayerType == PlayerType.PLAYER_OTHER)
				{
					carState.view.SyncController.Active = true;
				}
				else if (carState.CarPlayerType == PlayerType.PALYER_AI)
				{
					carState.view.Ai.Paused = false;
					if (controller != null)
					{
						controller.enabled = false;
					}
				}
			}
			if (soundObject != null)
			{
				UnityEngine.Object.Destroy(soundObject);
			}
		}
	}
}
