using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CarLogic
{
	public class ItemThrower : ItemTweener
	{
		private float halfTime;

		private float throwSpeed;

		private float angle = 360f;

		private float rotSpeed;

		private Vector3 orgPosition;

		private Vector3 vt1;

		private Quaternion orgRotation;

		private RaceItemId id = RaceItemId.ROCKET;

		private float accel = 12f;

		private float flowerHeight = 3.375f;

		private Vector3 normal;

		private bool dropDownGas = true;

		public const float LAY_OFFSET_Y = 1.05f;

		public ItemThrower(CarState state, float duration, Action callback, RaceItemId id)
			: base(state, duration, callback)
		{
			halfTime = duration / 2f;
			this.id = id;
		}

		public override void Start()
		{
			if (target != null)
			{
				startThrow();
			}
			base.Start();
		}

		public override void finish()
		{
			base.finish();
			if (target != null)
			{
				if (CameraController.Current != null && CameraController.Current.ViewTarget == target)
				{
					CameraController.Current.ResetCurRotDamp();
				}
				if (target.view != null)
				{
					target.view.WaitForDropDown();
					target.view.StartCoroutine(waitForGrounded(target));
				}
				if (target != null && target.CarPlayerType == PlayerType.PLAYER_SELF)
				{
					CarCallBack callBacks = target.CallBacks;
					callBacks.ResetChecker = (ModifyAction<bool>)Delegate.Remove(callBacks.ResetChecker, new ModifyAction<bool>(checker));
					CarCallBack callBacks2 = target.CallBacks;
					callBacks2.InverseChecker = (ModifyAction<bool>)Delegate.Remove(callBacks2.InverseChecker, new ModifyAction<bool>(checker));
				}
				if (target.view != null)
				{
					target.view.ItController.OnItemEffectEnd();
					CarView view = target.view;
					view.OnFixedupdate = (Action)Delegate.Remove(view.OnFixedupdate, new Action(throwUpdate));
				}
				if (target.CallBacks.PauseOnThrow != null)
				{
					target.CallBacks.PauseOnThrow(obj: false);
				}
				target = null;
			}
		}

		private IEnumerator waitForGrounded(CarState car)
		{
			while (car.GroundHit == 0)
			{
				yield return 0;
			}
			if (car.view != null && car.CallBacks.OnDropAfterThrow != null)
			{
				car.CallBacks.OnDropAfterThrow(car, dropDownGas);
			}
		}

		private void checker(ref bool enable)
		{
			if (target != null && target.CarPlayerType == PlayerType.PLAYER_SELF)
			{
				enable = false;
			}
		}

		private Quaternion getLayRot()
		{
			Vector3 eulerAngles = orgRotation.eulerAngles;
			eulerAngles.Set(0f, eulerAngles.y, 0f);
			return Quaternion.Euler(eulerAngles);
		}

		private Vector3 getLayPos()
		{
			if (target == null)
			{
				return Vector3.zero;
			}
			if (Physics.Raycast(orgPosition, Vector3.down, out var hitInfo, 100f, 256))
			{
				return hitInfo.point + new Vector3(0f, 1.05f, 0f);
			}
			return Vector3.zero;
		}

		private void throwUpdate()
		{
			if (target == null || target.view == null)
			{
				finish();
				return;
			}
			Transform transform = target.transform;
			float num = Time.time - startTime;
			if (num > halfTime + halfTime)
			{
				throwEnd();
				return;
			}
			if (num > halfTime && target.CarPlayerType == PlayerType.PLAYER_SELF && dropDownGas && target.CallBacks.DropDownGasChecker != null)
			{
				target.CallBacks.DropDownGasChecker(id, ref dropDownGas);
			}
			vt1 = orgPosition;
			vt1 += (throwSpeed * num - 0.5f * accel * num * num) * normal;
			transform.GetComponent<Rigidbody>().position = vt1;
			vt1.Set(1f, 0f, 0f);
			transform.transform.Rotate(vt1, rotSpeed * Time.deltaTime, Space.Self);
		}

		private void throwEnd()
		{
			finish();
		}

		private void startThrow()
		{
			if (target != null && !(target.view == null))
			{
				orgPosition = target.rigidbody.position + new Vector3(0f, 0.2f, 0f);
				orgRotation = target.rigidbody.rotation;
				CarView view = target.view;
				view.OnFixedupdate = (Action)Delegate.Combine(view.OnFixedupdate, new Action(throwUpdate));
				target.view.ItController.OnItemEffectStart();
				Vector3 eulerAngles = orgRotation.eulerAngles;
				eulerAngles.Set(0f, eulerAngles.y, 0f);
				target.rigidbody.rotation = Quaternion.Euler(eulerAngles);
				normal = target.LastNormal;
				startTime = Time.time;
				accel = 2f * flowerHeight / (halfTime * halfTime);
				throwSpeed = accel * halfTime;
				rotSpeed = angle / halfTime;
				if (CameraController.Current != null && CameraController.Current.ViewTarget == target)
				{
					CameraController.Current.SetCurRotDamp(0f);
					CameraController.Current.ShakeCamera(CameraController.ShakeType.Y, 0.5f);
				}
				if (target != null && target.CarPlayerType == PlayerType.PLAYER_SELF)
				{
					CarCallBack callBacks = target.CallBacks;
					callBacks.ResetChecker = (ModifyAction<bool>)Delegate.Combine(callBacks.ResetChecker, new ModifyAction<bool>(checker));
					CarCallBack callBacks2 = target.CallBacks;
					callBacks2.InverseChecker = (ModifyAction<bool>)Delegate.Combine(callBacks2.InverseChecker, new ModifyAction<bool>(checker));
				}
				List<RaceItemBase> list = new List<RaceItemBase>(4);
				target.view.ItController.ApplyingItem(RaceItemId.GAS, list);
				target.view.ItController.ApplyingItem(RaceItemId.GROUP_GAS, list);
				for (int i = 0; i < list.Count; i++)
				{
					list[i]?.Break();
				}
				List<PassiveTrigger> list2 = new List<PassiveTrigger>(4);
				target.view.ItController.ApplyingTrigger(RaceItemId.BANANA, list2);
				for (int j = 0; j < list2.Count; j++)
				{
					list2[j]?.Stop();
				}
				if (target.CallBacks.PauseOnThrow != null)
				{
					target.CallBacks.PauseOnThrow(obj: true);
				}
			}
		}
	}
}
