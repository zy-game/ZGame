using UnityEngine;

namespace CarLogic
{
	public class RaceStormItem : RaceItemBase
	{
		private const string objName = "TriggerStorm";

		private float _distance = 100f;

		private float _speed = 1500f;

		private float _yFactor = 15f;

		private float _xFactor = 2f;

		private float _duration = 5f;

		public float Duration => _duration;

		public override RaceItemId ItemId => RaceItemId.STORM;

		public RaceStormItem(RaceItemParameters param)
		{
			_duration = param.LifeTime;
			_distance = float.Parse(param.Params[0]);
			_speed = float.Parse(param.Params[1]);
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
			Singleton<ResourceOffer>.Instance.Load(RaceConfig.ItemStorm, delegate(Object o)
			{
				GameObject gameObject = Object.Instantiate(o) as GameObject;
				if (!(gameObject == null))
				{
					gameObject.transform.position = pos;
					gameObject.name = "TriggerStorm";
					getRoadInfo(ps.user, pos, out var width, out var up, out var right, out var midPos);
					width -= _xFactor;
					gameObject.transform.rotation = Quaternion.LookRotation(Vector3.Cross(right, up), up);
					gameObject.transform.position = midPos;
					ItemEllipseMover itemEllipseMover = new ItemEllipseMover(ps.user, gameObject.transform, width, _yFactor, _speed);
					itemEllipseMover.Start();
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
						carState.view.ExSource.PlayOneShot(RaceAudioManager.ActiveInstance.Sound_storm);
					}
				}
			});
		}

		public void getRoadInfo(CarState car, Vector3 pos, out float width, out Vector3 up, out Vector3 right, out Vector3 midPos)
		{
			right = car.transform.right;
			up = car.transform.up;
			RacePathNode racePathNode = car.CurNode;
			RacePathNode leftNode = car.CurNode.LeftNode;
			float num = 1f;
			while (racePathNode != null && leftNode != null)
			{
				float sqrMagnitude = (leftNode.transform.position - racePathNode.transform.position).sqrMagnitude;
				float sqrMagnitude2 = (pos - racePathNode.transform.position).sqrMagnitude;
				float sqrMagnitude3 = (pos - leftNode.transform.position).sqrMagnitude;
				if (sqrMagnitude2 + sqrMagnitude3 <= sqrMagnitude + 2f * num)
				{
					float num2 = Mathf.Sqrt(sqrMagnitude2 / sqrMagnitude);
					right = Vector3.Normalize(racePathNode.transform.right * num2 + leftNode.transform.right * (1f - num2));
					up = Vector3.Normalize(racePathNode.transform.up * num2 + leftNode.transform.up * (1f - num2));
					break;
				}
				right = racePathNode.transform.right;
				up = racePathNode.transform.up;
				racePathNode = leftNode;
				leftNode = leftNode.LeftNode;
			}
			int mask = LayerMask.GetMask("Wall");
			Vector3 vector = pos + right * 1f;
			Vector3 vector2 = pos - right * 1f;
			if (Physics.Raycast(pos, right, out var hitInfo, 100f, mask))
			{
				vector = hitInfo.point;
			}
			if (Physics.Raycast(pos, -right, out hitInfo, 100f, mask))
			{
				vector2 = hitInfo.point;
			}
			midPos = (vector2 + vector) * 0.5f;
			width = (vector2 - vector).magnitude;
		}
	}
}
