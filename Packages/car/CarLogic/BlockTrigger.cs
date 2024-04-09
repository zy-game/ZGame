using System;
using UnityEngine;

namespace CarLogic
{
	public class BlockTrigger : PassiveTrigger
	{
		private bool dragModified;

		private float dragPlus = 0.8f;

		private float startTime;

		private float _slowTime = 3f;

		private EffectToggleBase effectObject;

		private GameObject soundObject;

		public override RaceItemId ItemId => RaceItemId.BLOCK;

		public BlockTrigger(ushort id, long playerId = 0L)
			: base(id, playerId)
		{
			_slowTime = RaceItemFactory.GetDelayDuration(ItemId, isTrigger: true);
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
			startTime = Time.realtimeSinceStartup;
			if (carState.CarPlayerType == PlayerType.PLAYER_SELF)
			{
				RaceItemParameters itemParameters = RaceItemFactory.GetItemParameters(RaceItemId.BLOCK);
				CarCallBack callBacks = carState.CallBacks;
				callBacks.ResetChecker = (ModifyAction<bool>)Delegate.Combine(callBacks.ResetChecker, new ModifyAction<bool>(checker));
				carState.rigidbody.velocity *= float.Parse(itemParameters.Params[0]);
				carState.rigidbody.drag += dragPlus;
				dragModified = true;
			}
			else
			{
				PlayerType carPlayerType = carState.CarPlayerType;
				int num = 2;
			}
			Singleton<ResourceOffer>.Instance.Load(RaceConfig.ItemBlockAttach, delegate(UnityEngine.Object o)
			{
				if (!(carState.view == null) && !(null == o) && !(Time.realtimeSinceStartup - startTime >= _slowTime) && status != 0)
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
				soundObject = new GameObject("block sound");
				soundObject.transform.parent = carState.transform;
				soundObject.transform.localPosition = Vector3.zero;
				AudioSource audioSource = soundObject.AddComponent<AudioSource>();
				audioSource.clip = RaceAudioManager.ActiveInstance.Sound_hurt_block;
				audioSource.loop = false;
				audioSource.Play();
			}
			carState.view.CallDelay(Stop, RaceItemFactory.GetDelayDuration(RaceItemId.BLOCK, isTrigger: true));
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
			if (soundObject != null)
			{
				UnityEngine.Object.Destroy(soundObject);
			}
			if (carState == null)
			{
				return;
			}
			if (carState.CarPlayerType == PlayerType.PLAYER_SELF)
			{
				CarCallBack callBacks = carState.CallBacks;
				callBacks.ResetChecker = (ModifyAction<bool>)Delegate.Remove(callBacks.ResetChecker, new ModifyAction<bool>(checker));
				if (dragModified)
				{
					carState.rigidbody.drag -= dragPlus;
					dragModified = false;
				}
			}
			else
			{
				PlayerType carPlayerType = carState.CarPlayerType;
				int num = 2;
			}
		}
	}
}
