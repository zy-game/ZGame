using System;
using System.Collections.Generic;
using UnityEngine;

namespace CarLogic
{
	public class ItemFloter : ItemTweener
	{
		private AnimationCurve floatCurve = new AnimationCurve();

		private Vector3 vt1;

		private Vector3 orgPosition;

		private Vector3 normal;

		private Transform sqTransform;

		private float floatHeight = 1.05f;

		private float delay = 0.13f;

		private bool floating;

		private Quaternion leanAngle;

		private Quaternion orgRotation;

		private string effectPath = string.Empty;

		private static Quaternion[] floatAngles = new Quaternion[4]
		{
			Quaternion.Euler(-5f, 0f, 7f),
			Quaternion.Euler(9f, 0f, 5f),
			Quaternion.Euler(-10f, 0f, -5f),
			Quaternion.Euler(9f, 0f, -8f)
		};

		public ItemFloter(CarState state, AnimationCurve curve, string effectPath, Action callback = null, float duration = 3f, float floatHeight = 1.05f)
			: base(state, duration, callback)
		{
			floatCurve = curve;
			this.effectPath = effectPath;
			this.floatHeight = floatHeight;
		}

		public override void Start()
		{
			base.Start();
			if (target != null)
			{
				startFloat();
			}
		}

		public override void finish()
		{
			base.finish();
			if (target != null && target.view != null)
			{
				CarView view = target.view;
				view.OnFixedupdate = (Action)Delegate.Remove(view.OnFixedupdate, new Action(floatUpdate));
				target.view.ItController.OnItemEffectEnd();
				target.view.WaitForDropDown();
			}
			if (target != null && target.CarPlayerType == PlayerType.PLAYER_SELF)
			{
				CarCallBack callBacks = target.CallBacks;
				callBacks.ResetChecker = (ModifyAction<bool>)Delegate.Remove(callBacks.ResetChecker, new ModifyAction<bool>(checker));
				CarCallBack callBacks2 = target.CallBacks;
				callBacks2.InverseChecker = (ModifyAction<bool>)Delegate.Remove(callBacks2.InverseChecker, new ModifyAction<bool>(checker));
				if ((bool)target.rigidbody && !target.rigidbody.isKinematic)
				{
					target.rigidbody.velocity = Vector3.zero;
				}
			}
			target = null;
			if ((bool)sqTransform)
			{
				UnityEngine.Object.Destroy(sqTransform.gameObject);
			}
		}

		private void checker(ref bool enable)
		{
			if (target != null && target.CarPlayerType == PlayerType.PLAYER_SELF)
			{
				enable = false;
			}
		}

		private void floatUpdate()
		{
			if (target == null || target.view == null)
			{
				finish();
				return;
			}
			if (Time.time - startTime - delay > maxDuration)
			{
				endFloat();
				return;
			}
			if (!floating && Time.time - startTime > delay)
			{
				floating = true;
				orgPosition = target.rigidbody.position;
				orgRotation = target.rigidbody.rotation;
				normal = ((target.GroundHit != 0) ? target.transform.up : target.LastNormal);
				leanAngle = orgRotation * floatAngles[UnityEngine.Random.Range(0, floatAngles.Length - 1)];
				target.view.ItController.OnItemEffectStart(lockCamera: true);
			}
			if (floating)
			{
				vt1 = orgPosition;
				float time = (Time.time - startTime - delay) / maxDuration;
				time = floatCurve.Evaluate(time);
				vt1 += time * floatHeight * normal;
				target.rigidbody.position = vt1;
				target.rigidbody.rotation = Quaternion.Lerp(orgRotation, leanAngle, Mathf.Clamp01(time));
			}
		}

		private void startFloat()
		{
			if (target != null)
			{
				CarView view = target.view;
				view.OnFixedupdate = (Action)Delegate.Combine(view.OnFixedupdate, new Action(floatUpdate));
				if (target != null && target.CarPlayerType == PlayerType.PLAYER_SELF)
				{
					CarCallBack callBacks = target.CallBacks;
					callBacks.ResetChecker = (ModifyAction<bool>)Delegate.Combine(callBacks.ResetChecker, new ModifyAction<bool>(checker));
					CarCallBack callBacks2 = target.CallBacks;
					callBacks2.InverseChecker = (ModifyAction<bool>)Delegate.Combine(callBacks2.InverseChecker, new ModifyAction<bool>(checker));
				}
				orgPosition = target.rigidbody.position;
				Singleton<ResourceOffer>.Instance.Load(effectPath, delegate(UnityEngine.Object o)
				{
					if (target != null && !(target.view == null))
					{
						GameObject gameObject = UnityEngine.Object.Instantiate(o) as GameObject;
						sqTransform = gameObject.transform;
						sqTransform.parent = target.transform;
						sqTransform.localPosition = new Vector3(0f, 0.3f, 0f);
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
						target.view.PlayOnBubble();
					}
				});
			}
			else
			{
				finish();
			}
		}

		private void endFloat()
		{
			finish();
		}
	}
}
