using UnityEngine;

namespace CarLogic
{
	public class RaceCopsItemBase : RaceItemBase
	{
		protected virtual string objName => "Trigger";

		protected virtual string objPath => string.Empty;

		public override RaceItemId ItemId => RaceItemId.NONE;

		public virtual void ToggleNoBreak(ItemParams ps)
		{
			base.Toggle(ps);
			if (ps.user == null)
			{
				return;
			}
			CarState user = ps.user;
			ushort instanceId = ps.instanceId;
			Singleton<ResourceOffer>.Instance.Load(objPath, delegate(Object o)
			{
				if (!(o == null) && user != null && !(user.view == null))
				{
					GameObject gameObject = Object.Instantiate(o) as GameObject;
					gameObject.transform.position = ps.position;
					gameObject.transform.rotation = Quaternion.Euler(ps.rotation);
					gameObject.transform.localScale = ps.scale;
					gameObject.name = objName;
					SimpleData simpleData = gameObject.AddComponent<SimpleData>();
					TriggerData triggerData = (TriggerData)(simpleData.UserData = new TriggerData
					{
						LayTime = Time.time,
						ItemId = ItemId,
						InstanceId = instanceId,
						User = ps.user,
						TriggerObject = gameObject
					});
					saveToBuffer(triggerData);
					addReleaseCallback(simpleData, triggerData);
					loadObjFinish(gameObject, triggerData);
				}
			});
		}

		public override void Toggle(ItemParams ps)
		{
			ToggleNoBreak(ps);
			Break();
		}

		protected virtual void loadObjFinish(GameObject go, TriggerData td)
		{
			if (LoadFinishCallback != null)
			{
				LoadFinishCallback(go, td);
			}
		}

		public void BaseToggle(ItemParams ps)
		{
			base.Toggle(ps);
		}
	}
}
