using UnityEngine;

namespace CarLogic
{
	public class CopsBaseTrigger : PassiveTrigger
	{
		public bool WillDestroyAndAffect = true;

		protected GameObject soundObject;

		protected EffectToggleBase effectObject;

		protected float startTime;

		protected RaceItemParameters parameter;

		public override RaceItemId ItemId => RaceItemId.NONE;

		public virtual AudioClip AffectSound => null;

		public virtual string AffectEffect => string.Empty;

		public CopsBaseTrigger(ushort id, long playerWorldId = 0L)
			: base(id, playerWorldId)
		{
			parameter = RaceItemFactory.GetItemParameters(ItemId);
		}

		public virtual bool CopsApplyEffect(ItemParams ps)
		{
			return false;
		}

		public override void ApplyEffect(ItemParams ps)
		{
			base.ApplyEffect(ps);
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
			if (!WillDestroyAndAffect)
			{
				Stop();
				WillDestroyAndAffect = true;
				return;
			}
			startTime = Time.time;
			if (!CopsApplyEffect(ps))
			{
				Stop();
				return;
			}
			if (carState != null && carState.CallBacks.OnAffectedByItem != null)
			{
				carState.CallBacks.OnAffectedByItem(ItemId, carState, ItemCallbackType.AFFECT, this);
			}
			if (!string.IsNullOrEmpty(AffectEffect))
			{
				Singleton<ResourceOffer>.Instance.Load(AffectEffect, delegate(Object o)
				{
					if (!(carState.view == null) && !(null == o) && status != 0)
					{
						GameObject gameObject = Object.Instantiate(o) as GameObject;
						effectObject = gameObject.AddComponent<EffectToggleBase>();
						effectObject.Init();
						Transform transform = gameObject.transform;
						transform.parent = carState.transform;
						transform.localPosition = Vector3.zero;
						transform.localRotation = Quaternion.identity;
						effectObject.Active = true;
					}
				});
			}
			if (RaceAudioManager.ActiveInstance != null && AffectSound != null)
			{
				soundObject = new GameObject(ItemId.ToString());
				soundObject.transform.parent = carState.transform;
				soundObject.transform.localPosition = Vector3.zero;
				AudioSource audioSource = soundObject.AddComponent<AudioSource>();
				audioSource.clip = AffectSound;
				audioSource.loop = false;
				audioSource.Play();
			}
		}

		public override void Stop()
		{
			base.Stop();
			if (soundObject != null)
			{
				Object.Destroy(soundObject);
			}
			if (!(effectObject != null))
			{
				return;
			}
			effectObject.Active = false;
			RaceCallback.View.CallDelay(delegate
			{
				if (effectObject != null)
				{
					Object.Destroy(effectObject.gameObject);
				}
			}, 1f);
		}
	}
}
