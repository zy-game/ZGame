using System;
using UnityEngine;

namespace CarLogic
{
	public class CopsOilTrigger : CopsBaseTrigger
	{
		private Vector3 vt;

		private const float roundTimes = 3f;

		private float flipTime = 2f;

		private Vector3 dir;

		private Vector3 odir;

		private CharacterController controller;

		private float flipSpeed = 25f;

		private float rotateSpeed = 540f;

		private Vector3 up = new Vector3(0f, 1f, 0f);

		private Plane plane;

		public override RaceItemId ItemId => RaceItemId.COPS_OIL;

		public override AudioClip AffectSound => RaceAudioManager.ActiveInstance.Sound_xiangjiaopi;

		public override string AffectEffect => RaceConfig.CopsItemOilAttach;

		public CopsOilTrigger(ushort id, long playerId = 0L)
			: base(id, playerId)
		{
			flipTime = parameter.AffectedTime;
			rotateSpeed = 1080f / flipTime;
			plane = default(Plane);
		}

		public override bool CopsApplyEffect(ItemParams ps)
		{
			base.CopsApplyEffect(ps);
			flipSpeed = carState.velocity.magnitude * 0.5f;
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
			return true;
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

		private void affectChecker(RaceItemId id, ref bool usable)
		{
			if (usable && (id == RaceItemId.GAS || id == RaceItemId.GROUP_GAS))
			{
				usable = false;
			}
		}

		private void setState(CarState state)
		{
			state.Throttle = 0.01f;
			state.Steer = 0f;
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

		private void checker(ref bool enable)
		{
			enable = false;
		}

		public override void Stop()
		{
			base.Stop();
			CarState carState = base.carState;
			if (CameraController.Current != null && CameraController.Current.ViewTarget == base.carState)
			{
				CameraController.Current.ResetCurRotDamp();
				CameraController.Current.UseVelocityDir = false;
			}
			if (base.carState != null && base.carState.CarPlayerType == PlayerType.PLAYER_SELF)
			{
				CarCallBack callBacks = base.carState.CallBacks;
				callBacks.OnSetState = (Action<CarState>)Delegate.Remove(callBacks.OnSetState, new Action<CarState>(setState));
				CarCallBack callBacks2 = base.carState.CallBacks;
				callBacks2.OnApplyForce = (Action)Delegate.Remove(callBacks2.OnApplyForce, new Action(fixedupdate));
				CarCallBack callBacks3 = base.carState.CallBacks;
				callBacks3.InverseChecker = (ModifyAction<bool>)Delegate.Remove(callBacks3.InverseChecker, new ModifyAction<bool>(checker));
				CarCallBack callBacks4 = base.carState.CallBacks;
				callBacks4.ResetChecker = (ModifyAction<bool>)Delegate.Remove(callBacks4.ResetChecker, new ModifyAction<bool>(checker));
				base.carState.view.SyncController.ResetTimeLeft();
				if ((bool)base.carState.rigidbody && !base.carState.rigidbody.isKinematic)
				{
					base.carState.view.EnableSidewayFriction();
				}
				for (int i = 0; i < base.carState.Wheels.Length; i++)
				{
					Wheel wheel = base.carState.Wheels[i];
					JointSpring suspensionSpring = wheel.collider.suspensionSpring;
					suspensionSpring.spring /= 12f;
				}
				if (base.carState.ApplyingSpecialType != SpecialType.Translate)
				{
					base.carState.view.DriftEnable = true;
				}
				return;
			}
			CarView view = base.carState.view;
			view.OnFixedupdate = (Action)Delegate.Remove(view.OnFixedupdate, new Action(update));
			if (base.carState.CarPlayerType == PlayerType.PLAYER_OTHER)
			{
				base.carState.view.SyncController.Active = true;
			}
			else if (base.carState.CarPlayerType == PlayerType.PALYER_AI)
			{
				base.carState.view.Ai.Paused = false;
				if (controller != null)
				{
					controller.enabled = false;
				}
			}
		}
	}
}
