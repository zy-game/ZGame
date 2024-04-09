using System;
using System.Collections.Generic;
using UnityEngine;

namespace CarLogic
{
	public class RaceUFOItem : RaceItemBase
	{
		private float duration = 3f;

		private bool attackable;

		private float dragPlus = 0.8f;

		private float startTime;

		private GameObject ufoObject;

		private GameObject ufoModel;

		private float offset = -1f;

		private ItemFollower follower;

		private bool dragModified;

		private float throwDuration = 0.5f;

		private static Vector3[] fadeInPath = new Vector3[5]
		{
			new Vector3(4f, 3.8f, 0f),
			new Vector3(-2.8f, 2.6f, -8f),
			new Vector3(1.5f, 1.9f, 6.5f),
			new Vector3(-0.3f, 0.3f, 3.25f),
			new Vector3(0f, -0.2f, 0f)
		};

		private static Vector3[] fadeOutPath = new Vector3[2]
		{
			new Vector3(0f, -0.2f, 0f),
			new Vector3(0f, 20f, 0f)
		};

		public static float UFO_FADE_TIME = 0.8f;

		private float _ufoFadeInTime = UFO_FADE_TIME;

		private float _ufoFadeOutTime = UFO_FADE_TIME;

		private ItemTranslator translator;

		public override RaceItemId ItemId => RaceItemId.UFO;

		public RaceUFOItem(RaceItemParameters param)
		{
			duration = param.AffectedTime;
		}

		protected override void init()
		{
			base.init();
		}

		public override void Toggle(ItemParams ps)
		{
			base.Toggle(ps);
			targets = ps.targets;
			if (targets == null || targets.Length < 1 || !targets[0].view.gameObject.activeSelf)
			{
				Break();
				return;
			}
			Singleton<ResourceOffer>.Instance.Load(RaceConfig.ItemUFO, delegate(UnityEngine.Object obj)
			{
				if (!(obj == null) && status != 0 && targets[0].view.gameObject.activeSelf)
				{
					ufoModel = UnityEngine.Object.Instantiate(obj) as GameObject;
					ufoModel.name = "ufomodel";
					ufoObject = new GameObject("ufo");
					ufoModel.transform.parent = ufoObject.transform;
					ufoModel.transform.localPosition = Vector3.zero;
					ufoModel.transform.localRotation = Quaternion.identity;
					ufoObject.transform.parent = carState.transform;
					ufoObject.transform.localRotation = Quaternion.identity;
					Throwing();
				}
			});
		}

		private void Throwing()
		{
			Transform transform = ufoObject.transform;
			Vector3[] array = new Vector3[3]
			{
				new Vector3(0f, -1.2f, 0f),
				new Vector3(0f, -1.4f, 0f),
				new Vector3(0f, 1f, 0f)
			};
			transform.position = array[0];
			MessageDelegate messageDelegate = ufoObject.AddComponent<MessageDelegate>();
			messageDelegate.AcOnFinish = (Action)Delegate.Combine(messageDelegate.AcOnFinish, new Action(ThrowFinish));
			Animator component = ufoModel.GetComponent<Animator>();
			component.SetInteger("state", 1);
			RaceCallback.TweenMove(ufoObject, new object[10] { "path", array, "time", throwDuration, "easetype", "linear", "oncomplete", "OnFinish", "islocal", true });
		}

		private void ThrowFinish()
		{
			MessageDelegate component = ufoObject.GetComponent<MessageDelegate>();
			component.AcOnFinish = (Action)Delegate.Remove(component.AcOnFinish, new Action(ThrowFinish));
			ufoObject.transform.parent = targets[0].transform;
			FadeIn();
		}

		private void FinishCallback()
		{
			Animator component = ufoModel.GetComponent<Animator>();
			if (targets == null || targets.Length == 0 || targets[0] == null || targets[0].view == null)
			{
				return;
			}
			List<RaceItemBase> list = new List<RaceItemBase>(4);
			attackable = targets[0] != itemParams.user;
			if (targets[0].CallBacks.AffectChecker != null)
			{
				targets[0].CallBacks.AffectChecker(ItemId, ref attackable);
			}
			if (targets[0] != null && targets[0].CallBacks.OnAffectedByItem != null)
			{
				targets[0].CallBacks.OnAffectedByItem(ItemId, targets[0], ItemCallbackType.CHECK, this);
			}
			if (!attackable)
			{
				duration = 0.2f;
			}
			CarView view = targets[0].view;
			view.OnUpdate = (Action)Delegate.Combine(view.OnUpdate, new Action(update));
			if (targets[0].view.ItController.ApplyingItem(RaceItemId.UFO_CONFUSER, list))
			{
				for (int i = 0; i < list.Count; i++)
				{
					RaceItemBase raceItemBase = list[i];
					raceItemBase.Break();
				}
			}
			list.Clear();
			if (attackable)
			{
				if (targets[0].CallBacks.OnAffectedByItem != null)
				{
					targets[0].CallBacks.OnAffectedByItem(ItemId, targets[0], ItemCallbackType.AFFECT, this);
				}
				targets[0].view.PlayOnUFO();
			}
			if (targets[0].view.ItController.ApplyingExcept(this, list))
			{
				for (int j = 0; j < list.Count; j++)
				{
					RaceItemBase raceItemBase2 = list[j];
					raceItemBase2.Enhance(duration);
				}
				Break();
				return;
			}
			if (targets[0].CarPlayerType == PlayerType.PLAYER_SELF && attackable)
			{
				dragModified = true;
				targets[0].rigidbody.drag += dragPlus;
			}
			startTime = Time.time;
			if (attackable && component != null)
			{
				component.SetInteger("state", 0);
			}
		}

		private void FadeIn()
		{
			if (targets[0].transform == null)
			{
				UnityEngine.Object.Destroy(ufoObject);
				Break();
				return;
			}
			follower = new ItemFollower(targets[0], ufoObject.transform, offset);
			follower.Start();
			Transform transform = ufoObject.transform;
			Animator component = ufoModel.GetComponent<Animator>();
			component.SetInteger("state", -1);
			Vector3[] array = fadeInPath;
			transform.localPosition = array[0];
			RaceCallback.TweenMove(ufoObject, new object[10] { "path", array, "time", _ufoFadeInTime, "easetype", "linear", "oncomplete", "OnFinish", "islocal", true });
			MessageDelegate component2 = ufoObject.GetComponent<MessageDelegate>();
			component2.AcOnFinish = FinishCallback;
			if (RaceAudioManager.ActiveInstance != null)
			{
				AudioSource audioSource = ufoObject.AddComponent<AudioSource>();
				audioSource.clip = RaceAudioManager.ActiveInstance.Sound_feidie;
				audioSource.loop = true;
				audioSource.Play();
			}
		}

		private void update()
		{
			if (Time.time - startTime > duration + exhanceTime)
			{
				Break();
			}
		}

		private void effectEnd()
		{
			if (targets != null && targets.Length >= 1 && targets[0].CarPlayerType == PlayerType.PLAYER_SELF && dragModified && (bool)targets[0].rigidbody)
			{
				targets[0].rigidbody.drag -= dragPlus;
			}
		}

		public override void Enhance(float addition)
		{
			startTime = Time.time;
		}

		private void FadeOut()
		{
			Transform transform = ufoObject.transform;
			Vector3[] array = new Vector3[fadeOutPath.Length];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = transform.TransformPoint(fadeOutPath[i]);
			}
			transform.position = array[0];
			Animator component = ufoModel.GetComponent<Animator>();
			component.SetInteger("state", 1);
			RaceCallback.TweenMove(ufoObject, new object[8] { "path", array, "time", _ufoFadeOutTime, "easetype", "linear", "islocal", false });
		}

		public void SetFadeOutTime(float fadeOutTime)
		{
			if (null != ufoObject)
			{
				RaceCallback.TweenStop(ufoObject);
			}
			_ufoFadeOutTime = fadeOutTime;
		}

		public override void Break()
		{
			base.Break();
			if (targets == null || targets.Length < 1)
			{
				if (null != ufoObject)
				{
					UnityEngine.Object.Destroy(ufoObject);
				}
				return;
			}
			effectEnd();
			if (targets[0].view != null)
			{
				CarView view = targets[0].view;
				view.OnUpdate = (Action)Delegate.Remove(view.OnUpdate, new Action(update));
			}
			if (!(ufoObject == null))
			{
				if (follower != null)
				{
					follower.finish();
				}
				FadeOut();
				if (null != ufoObject)
				{
					UnityEngine.Object.Destroy(ufoObject, _ufoFadeOutTime);
				}
			}
		}
	}
}
