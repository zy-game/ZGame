using UnityEngine;

namespace CarLogic
{
	public class RaceFogItem : RaceItemBase
	{
		private float _duration = 5f;

		public virtual float Duration => _duration;

		public override RaceItemId ItemId => RaceItemId.FOG;

		public RaceFogItem(RaceItemParameters param)
		{
			_duration = param.LifeTime;
		}

		public override void Toggle(ItemParams ps)
		{
			base.Toggle(ps);
			Singleton<ResourceOffer>.Instance.Load(RaceConfig.ItemFog, delegate(Object o)
			{
				if (!(o == null))
				{
					GameObject gameObject = Object.Instantiate(o) as GameObject;
					if (ps != null)
					{
						gameObject.transform.position = ps.position;
						if (ps.user != null)
						{
							gameObject.transform.rotation = ps.user.CurNode.transform.rotation;
						}
					}
					Object.Destroy(gameObject, Duration);
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
