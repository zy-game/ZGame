using CarLogic;
using UnityEngine;

public class RaceBoss1BananaItem : RaceItemBase
{
	private const string objName = "TriggerBoss1Banana";

	public override RaceItemId ItemId => RaceItemId.BANANA;

	public override void Toggle(ItemParams ps)
	{
		base.Toggle(ps);
		if (ps.user == null)
		{
			Break();
			return;
		}
		CarState user = ps.user;
		ushort instanceId = ps.instanceId;
		Singleton<ResourceOffer>.Instance.Load(RaceConfig.ItemBanana, delegate(Object o)
		{
			if (!(o == null) && user != null && !(user.view == null))
			{
				RaceBoss1MineItem.CurGroupIndex = (int)(++RaceBoss1MineItem.CurGroupIndex) % RaceBoss1MineItem.PosGroup.GetLength(0);
				for (int i = 0; i < 3; i++)
				{
					GameObject gameObject = Object.Instantiate(o) as GameObject;
					Vector3 lastNormal = user.LastNormal;
					Vector3 vector = RaceBoss1MineItem.PosGroup[(int)RaceBoss1MineItem.CurGroupIndex, i, 0] * user.transform.right;
					Vector3 vector2 = RaceBoss1MineItem.PosGroup[(int)RaceBoss1MineItem.CurGroupIndex, i, 1] * user.transform.forward;
					Vector3 vector3 = user.transform.position + vector2 + vector;
					if (Physics.Raycast(vector3 + lastNormal * 3f, -lastNormal, out var hitInfo, 100f, 256))
					{
						vector3 = hitInfo.point;
						lastNormal = hitInfo.normal;
						gameObject.transform.position = vector3;
						gameObject.transform.rotation = Quaternion.LookRotation(user.transform.forward, lastNormal);
						gameObject.name = "TriggerBoss1Banana";
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
					else
					{
						Object.DestroyImmediate(gameObject);
					}
				}
			}
		});
		Break();
	}
}
