using System;
using System.Collections.Generic;
using UnityEngine;

namespace CarLogic
{
	public class RaceAntiUFOItem : RaceItemBase
	{
		private float duration = 2f;

		private float startTime;

		private float offset = -1f;

		private GameObject ufoObject;

		private ItemFollower follower;

		public override RaceItemId ItemId => RaceItemId.UFO_CONFUSER;

		public override void Toggle(ItemParams ps)
		{
			base.Toggle(ps);
			targets = ps.targets;
			if (targets == null || targets.Length < 1)
			{
				base.Toggle(ps);
				Break();
				return;
			}
			List<RaceItemBase> list = new List<RaceItemBase>(4);
			if (targets[0].view.ItController.ApplyingItem(RaceItemId.UFO, list))
			{
				for (int i = 0; i < list.Count; i++)
				{
					RaceItemBase raceItemBase = list[i];
					raceItemBase.Break();
				}
				Singleton<ResourceOffer>.Instance.Load(RaceConfig.ItemAntiUFO, delegate(UnityEngine.Object o)
				{
					GameObject gameObject = UnityEngine.Object.Instantiate(o) as GameObject;
					if (!(gameObject == null))
					{
						ufoObject = gameObject;
						follower = new ItemFollower(targets[0], ufoObject.transform, offset);
						follower.Start();
						if (targets[0] != null && targets[0].CallBacks.OnAffectedByItem != null)
						{
							targets[0].CallBacks.OnAffectedByItem(ItemId, targets[0], ItemCallbackType.AFFECT, this);
						}
						if (RaceAudioManager.ActiveInstance != null)
						{
							AudioSource audioSource = ufoObject.AddComponent<AudioSource>();
							audioSource.clip = RaceAudioManager.ActiveInstance.Sound_duanlu;
							audioSource.loop = true;
							audioSource.Play();
						}
					}
				});
				startTime = Time.time;
				CarView view = targets[0].view;
				view.OnUpdate = (Action)Delegate.Combine(view.OnUpdate, new Action(update));
			}
			else
			{
				Break();
			}
		}

		private void update()
		{
			if (Time.time - startTime > duration)
			{
				Break();
			}
		}

		public override void Break()
		{
			base.Break();
			if (follower != null)
			{
				follower.finish();
			}
			if (targets != null && targets.Length >= 1)
			{
				CarView view = targets[0].view;
				view.OnUpdate = (Action)Delegate.Remove(view.OnUpdate, new Action(update));
				if ((bool)ufoObject)
				{
					UnityEngine.Object.Destroy(ufoObject);
				}
			}
		}
	}
}
