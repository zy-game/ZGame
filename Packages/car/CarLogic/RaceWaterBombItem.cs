using UnityEngine;

namespace CarLogic
{
	public class RaceWaterBombItem : RaceItemBase
	{
		private float Distance = 100f;

		private const string objName = "TriggerWaterbomb";

		private float _duration = 2.2f;

		public float Duration => _duration;

		public override RaceItemId ItemId => RaceItemId.WATER_BOMB;

		public RaceWaterBombItem(RaceItemParameters param)
		{
			_duration = param.LifeTime;
			Distance = float.Parse(param.Params[0]);
		}

		public override void Toggle(ItemParams ps)
		{
			base.Toggle(ps);
			targets = ps.targets;
			if (ps.user == null)
			{
				Break();
				return;
			}
			Vector3 pos = ps.position;
			ushort instanceId = ps.instanceId;
			Singleton<ResourceOffer>.Instance.Load(RaceConfig.ItemWaterBomb, delegate(Object o)
			{
				GameObject gameObject = Object.Instantiate(o) as GameObject;
				if (!(gameObject == null))
				{
					gameObject.transform.position = pos;
					gameObject.name = "TriggerWaterbomb";
					SimpleData simpleData = gameObject.AddComponent<SimpleData>();
					TriggerData data = (TriggerData)(simpleData.UserData = new TriggerData
					{
						LayTime = Time.time,
						ItemId = ItemId,
						InstanceId = instanceId,
						User = ps.user,
						TriggerObject = gameObject
					});
					saveToBuffer(data);
					addReleaseCallback(simpleData, data);
					Object.Destroy(gameObject, Duration);
					Break();
					if (RaceAudioManager.ActiveInstance != null)
					{
						AudioSource audioSource = gameObject.AddComponent<AudioSource>();
						audioSource.clip = RaceAudioManager.ActiveInstance.Sound_shuipao;
						audioSource.Play();
					}
				}
			});
		}

		public override void Break()
		{
			base.Break();
		}
	}
}
