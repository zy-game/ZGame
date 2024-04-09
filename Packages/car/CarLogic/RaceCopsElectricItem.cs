using UnityEngine;

namespace CarLogic
{
	public class RaceCopsElectricItem : RaceCopsItemBase
	{
		protected GameObject preObj;

		protected const string PreObjPath = "Effects/Sence/CopsPreElectric";

		protected float _flySpeed;

		protected override string objName => "TriggerElectric";

		protected override string objPath => RaceConfig.CopsItemElectric;

		public override RaceItemId ItemId => RaceItemId.COPS_ELECTRIC;

		public override void Toggle(ItemParams ps)
		{
			status = RaceItemStatus.APPLYING;
			carState = ps.user;
			targets = ps.targets;
			RaceItemParameters parameter = RaceItemFactory.GetItemParameters(ItemId);
			itemParams = new ItemParams(ps);
			float shotTime = float.Parse(parameter.Params[0]);
			CarState user = itemParams.user;
			CarState target = itemParams.targets[0];
			float num = 0.5f;
			Vector3 targetPosition = getTargetPosition(target, shotTime + num);
			Vector3 normalized = (targetPosition - user.transform.position).normalized;
			itemParams.position = user.transform.position;
			itemParams.rotation = Quaternion.LookRotation(normalized).eulerAngles;
			itemParams.scale = Vector3.one;
			ushort instanceId = itemParams.instanceId;
			Singleton<ResourceOffer>.Instance.Load("Effects/Sence/CopsPreElectric", delegate(Object o)
			{
				if (!(o == null) && user != null && !(user.view == null) && target != null && !(target.view == null))
				{
					float num2 = float.Parse(parameter.Params[1]);
					float num3 = float.Parse(parameter.Params[2]);
					float num4 = num3 * 0.5f;
					_flySpeed = num3 / (float)parameter.LifeTime;
					GameObject gameObject = Object.Instantiate(o) as GameObject;
					gameObject.transform.position = itemParams.position;
					gameObject.transform.rotation = Quaternion.Euler(itemParams.rotation);
					gameObject.name = "TriggerPreElectric";
					preObj = gameObject;
					Projector componentInChildren = gameObject.GetComponentInChildren<Projector>();
					if (componentInChildren != null)
					{
						componentInChildren.orthographicSize = num4;
						componentInChildren.aspectRatio = num2 / num4;
						Vector3 localPosition = componentInChildren.transform.localPosition;
						localPosition.z = num4;
						componentInChildren.transform.localPosition = localPosition;
					}
					AbstractView abstractView = gameObject.GetComponent<AbstractView>();
					if (abstractView == null)
					{
						abstractView = gameObject.AddComponent<AbstractView>();
					}
					abstractView.CallDelay(baseToggle, shotTime);
				}
			});
		}

		protected void baseToggle()
		{
			base.ToggleNoBreak(itemParams);
			itemParams.targets = null;
			Object.Destroy(preObj);
		}

		protected override void loadObjFinish(GameObject go, TriggerData td)
		{
			base.loadObjFinish(go, td);
			if (!(go != null))
			{
				return;
			}
			RaceItemParameters itemParameters = RaceItemFactory.GetItemParameters(ItemId);
			AbstractView abstractView = go.GetComponent<AbstractView>();
			if (abstractView == null)
			{
				abstractView = go.AddComponent<AbstractView>();
			}
			abstractView.CallDelay(delegate
			{
				if (go != null)
				{
					Object.Destroy(go);
				}
				Break();
			}, itemParameters.LifeTime);
			ItemMover itemMover = new ItemMover(carState, go.transform, go.transform.forward, _flySpeed, itemParameters.LifeTime);
			itemMover.Start();
			if (RaceAudioManager.ActiveInstance != null)
			{
				AudioSource audioSource = go.AddComponent<AudioSource>();
				audioSource.clip = RaceAudioManager.ActiveInstance.Sound_Electric;
				audioSource.loop = true;
				audioSource.maxDistance = 20f;
				audioSource.minDistance = 10f;
				audioSource.Play();
			}
		}

		protected Vector3 getTargetPosition(CarState target, float t)
		{
			if (target == null || null == target.CurNode || null == target.transform || null == target.CurNode.transform)
			{
				return Vector3.zero;
			}
			RacePathNode curNode = target.CurNode;
			RacePathNode leftNode = target.CurNode.LeftNode;
			if (leftNode == null || null == leftNode.transform)
			{
				return curNode.transform.position;
			}
			float num = target.LinearVelocity * t;
			Vector3 vector = leftNode.transform.position - curNode.transform.position;
			Vector3 vector2 = target.transform.position + vector.normalized * num;
			Ray ray = new Ray(vector2 + curNode.transform.up * 3f, -curNode.transform.up);
			if (Physics.Raycast(ray, out var hitInfo, 10f, LayerMask.NameToLayer("Road")))
			{
				vector2 = hitInfo.point + hitInfo.normal * 0.5f;
			}
			return vector2;
		}
	}
}
