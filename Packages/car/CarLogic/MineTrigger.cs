using System;
using UnityEngine;

namespace CarLogic
{
	public class MineTrigger : PassiveTrigger
	{
		private ItemThrower thrower;

		public override RaceItemId ItemId => RaceItemId.MINE;

		public MineTrigger(ushort id, long playerId = 0L)
			: base(id, playerId)
		{
		}

		public override void ApplyEffect(ItemParams ps)
		{
			base.ApplyEffect(ps);
			Release();
			if (carState == null)
			{
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
			Singleton<ResourceOffer>.Instance.Load(RaceConfig.ItemExplosion, delegate(UnityEngine.Object o)
			{
				if (!(o == null))
				{
					GameObject gameObject = UnityEngine.Object.Instantiate(o) as GameObject;
					CarState carState = base.carState;
					Vector3 vector = carState.LastNormal;
					Vector3 position = carState.transform.position;
					if (Physics.Raycast(position + vector * 3f, -vector, out var hitInfo, 100f, 256))
					{
						vector = hitInfo.normal;
					}
					gameObject.transform.position = position;
					gameObject.transform.rotation = Quaternion.LookRotation(carState.transform.forward, vector);
					if (RaceAudioManager.ActiveInstance != null)
					{
						AudioSource audioSource = gameObject.AddComponent<AudioSource>();
						audioSource.clip = RaceAudioManager.ActiveInstance.Sound_daodanjizhong;
						audioSource.Play();
					}
					UnityEngine.Object.Destroy(gameObject, 1.8f);
				}
			});
			if (carState != null)
			{
				CarCallBack callBacks = carState.CallBacks;
				callBacks.AffectChecker = (ModifyAction<RaceItemId, bool>)Delegate.Combine(callBacks.AffectChecker, new ModifyAction<RaceItemId, bool>(affectChecker));
			}
			thrower = new ItemThrower(carState, RaceItemFactory.GetDelayDuration(ItemId, isTrigger: true), Stop, ItemId);
			thrower.Start();
			if (carState.CallBacks.OnFreezeMoveEffect != null)
			{
				carState.CallBacks.OnFreezeMoveEffect(ItemId, RaceItemFactory.GetDelayDuration(ItemId, isTrigger: true));
			}
			if (carState != null && carState.CallBacks.OnAffectedByItem != null)
			{
				carState.CallBacks.OnAffectedByItem(ItemId, carState, ItemCallbackType.AFFECT, this);
			}
		}

		private void affectChecker(RaceItemId id, ref bool usable)
		{
			if (usable && (id == RaceItemId.GAS || id == RaceItemId.GROUP_GAS))
			{
				usable = false;
			}
		}

		public override void Release()
		{
			base.Release();
		}

		public override void Stop()
		{
			if (thrower != null)
			{
				thrower.finish();
			}
			if (carState != null)
			{
				CarCallBack callBacks = carState.CallBacks;
				callBacks.AffectChecker = (ModifyAction<RaceItemId, bool>)Delegate.Remove(callBacks.AffectChecker, new ModifyAction<RaceItemId, bool>(affectChecker));
			}
			base.Stop();
		}
	}
}
