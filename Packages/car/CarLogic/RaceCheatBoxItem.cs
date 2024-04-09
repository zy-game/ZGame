using UnityEngine;

namespace CarLogic
{
	public class RaceCheatBoxItem : RaceItemBase
	{
		private const string objName = "TriggerCheatbox";

		private string prefabPath = string.Empty;

		public override RaceItemId ItemId => RaceItemId.CHEAT_BOX;

		public RaceCheatBoxItem(RaceItemParameters param)
		{
			prefabPath = "Effects/Sence/" + param.Params[0];
		}

		public override void Toggle(ItemParams ps)
		{
			base.Toggle(ps);
			Singleton<ResourceOffer>.Instance.Load(prefabPath, delegate(Object o)
			{
				CarState user = ps.user;
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
					gameObject.transform.position = vector2 + vector * 0.2f;
					gameObject.transform.up = vector;
					gameObject.name = "TriggerCheatbox";
					Collider componentInChildren = gameObject.GetComponentInChildren<Collider>();
					if ((bool)componentInChildren)
					{
						componentInChildren.gameObject.layer = 12;
					}
					SimpleData simpleData = gameObject.AddComponent<SimpleData>();
					TriggerData data = (TriggerData)(simpleData.UserData = new TriggerData
					{
						LayTime = Time.time,
						ItemId = ItemId,
						InstanceId = ps.instanceId,
						User = user,
						TriggerObject = gameObject
					});
					saveToBuffer(data);
					addReleaseCallback(simpleData, data);
				}
			});
			Break();
		}

		public override void Break()
		{
			base.Break();
		}
	}
}
