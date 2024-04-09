using UnityEngine;

namespace CarLogic
{
	public class ItemParams
	{
		public Vector3 position;

		public Vector3 rotation;

		public Vector3 scale = Vector3.one;

		public CarState[] targets;

		public CarState user;

		public ushort instanceId;

		public float DirY;

		public ItemParams(CarState[] targets = null, CarState user = null, ushort instanceId = 0)
		{
			this.targets = targets;
			this.user = user;
			this.instanceId = instanceId;
			position = new Vector3(0f, 0f, 0f);
			rotation = Vector3.zero;
			scale = Vector3.one;
			DirY = 0f;
		}

		public ItemParams(ItemParams o)
		{
			targets = o.targets;
			user = o.user;
			DirY = o.DirY;
			instanceId = o.instanceId;
			position = o.position;
			rotation = o.rotation;
			scale = o.scale;
		}
	}
}
