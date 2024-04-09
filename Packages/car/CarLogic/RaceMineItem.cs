using UnityEngine;

namespace CarLogic
{
	public class RaceMineItem : RaceItemBase
	{
		private const string objName = "TriggerMine";

		public override RaceItemId ItemId => RaceItemId.MINE;

		public override void Toggle(ItemParams ps)
		{
			base.Toggle(ps);
			if (ps.user != null)
			{
				CarState user = ps.user;
				ushort instanceId = ps.instanceId;
				Singleton<ResourceOffer>.Instance.Load(RaceConfig.ItemMine, delegate(Object o)
				{
					if (!(o == null) && user != null && !(user.view == null))
					{
						GameObject gameObject = Object.Instantiate(o) as GameObject;
						if (user != null)
						{
							Vector3 vector = user.LastNormal;
							Vector3 vector2 = user.transform.position - 1.2f * user.transform.forward;
							if (Physics.Raycast(vector2 + vector * 3f, -vector, out var hitInfo, 100f, 256))
							{
								vector2 = hitInfo.point;
								vector = hitInfo.normal;
							}
							gameObject.transform.position = vector2;
							gameObject.transform.rotation = Quaternion.LookRotation(user.transform.forward, vector);
							gameObject.name = "TriggerMine";
							SimpleData simpleData = gameObject.AddComponent<SimpleData>();
							TriggerData data = (TriggerData)(simpleData.UserData = new TriggerData
							{
								LayTime = Time.time,
								ItemId = ItemId,
								InstanceId = instanceId,
								User = user,
								TriggerObject = gameObject
							});
							saveToBuffer(data);
							addReleaseCallback(simpleData, data);
						}
						else
						{
							Debug.LogWarning("Null User on use Mine.");
						}
					}
				});
			}
			Break();
		}

		public override void Break()
		{
			base.Break();
		}
	}
}
