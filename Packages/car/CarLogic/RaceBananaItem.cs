using UnityEngine;

namespace CarLogic
{
	public class RaceBananaItem : RaceItemBase
	{
		private const string objName = "TriggerBanana";

		public override RaceItemId ItemId => RaceItemId.BANANA;

		protected override void init()
		{
			base.init();
		}

		public override void Toggle(ItemParams ps)
		{
			base.Toggle(ps);
			if (ps.user != null)
			{
				CarState user = ps.user;
				ushort instanceId = ps.instanceId;
				Singleton<ResourceOffer>.Instance.Load(RaceConfig.ItemBanana, delegate(Object o)
				{
					if (!(o == null) && user != null && !(user.view == null))
					{
						GameObject gameObject = Object.Instantiate(o) as GameObject;
						Vector3 vector = user.LastNormal;
						Vector3 vector2 = user.transform.position - 1.2f * user.transform.forward;
						if (Physics.Raycast(vector2 + vector * 3f, -vector, out var hitInfo, 100f, 256))
						{
							vector2 = hitInfo.point;
							vector = hitInfo.normal;
						}
						gameObject.transform.position = vector2;
						gameObject.transform.rotation = Quaternion.LookRotation(user.transform.forward, vector);
						gameObject.name = "TriggerBanana";
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
