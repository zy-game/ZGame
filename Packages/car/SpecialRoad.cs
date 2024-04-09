using CarLogic;
using UnityEngine;

public class SpecialRoad : MonoBehaviour
{
	private const float DefaultFrictionFactor = 1f;

	private const string SpecialEffectName = "SpecialWheelEffect";

	private const string EffectName = "WheelEffect";

	public Object WheelEffectPrefab;

	public float FrictionScale = 0.8f;

	public LayerMask CheckLayerMask;

	private EffectToggleBase[] LoadSpecialWheelEffect(CarView carView)
	{
		GameObject gameObject = new GameObject();
		gameObject.name = "SpecialWheelEffect";
		gameObject.transform.parent = carView.transform;
		gameObject.transform.localScale = Vector3.one;
		gameObject.transform.localPosition = Vector3.zero;
		gameObject.transform.localRotation = Quaternion.identity;
		EffectAnchor[] wheelEffectAnchors = carView.WheelEffectAnchors;
		EffectToggleBase[] array = new EffectToggleBase[wheelEffectAnchors.Length];
		for (int i = 0; i < wheelEffectAnchors.Length; i++)
		{
			GameObject gameObject2 = Object.Instantiate(WheelEffectPrefab) as GameObject;
			gameObject2.transform.parent = gameObject.transform;
			gameObject2.transform.localPosition = wheelEffectAnchors[i].Position;
			gameObject2.transform.localRotation = wheelEffectAnchors[i].Rotation;
			DistanceParticleEffectToggle distanceParticleEffectToggle = gameObject2.AddComponent<DistanceParticleEffectToggle>();
			distanceParticleEffectToggle.Init();
			distanceParticleEffectToggle.IgnoreRenders = true;
			distanceParticleEffectToggle.Active = false;
			array[i] = distanceParticleEffectToggle;
		}
		carView.SkController.SpecialSparks = array;
		return carView.SkController.SpecialSparks;
	}

	private bool CheckLayer(GameObject obj)
	{
		int num = (int)CheckLayerMask & (1 << obj.layer);
		return num == 1 << obj.layer;
	}

	private void OnTriggerEnter(Collider other)
	{
		if (!CheckLayer(other.gameObject))
		{
			return;
		}
		CarView componentInParent = other.GetComponentInParent<CarView>();
		if (!(null != componentInParent))
		{
			return;
		}
		if (null != WheelEffectPrefab)
		{
			EffectToggleBase[] array = componentInParent.SkController.SpecialSparks;
			if (array == null)
			{
				array = LoadSpecialWheelEffect(componentInParent);
			}
			for (int i = 0; i < array.Length; i++)
			{
				array[i].Active = true;
			}
		}
		componentInParent.carModel.GroundFrictionScale = FrictionScale;
	}

	private void OnTriggerExit(Collider other)
	{
		if (!CheckLayer(other.gameObject))
		{
			return;
		}
		CarView componentInParent = other.GetComponentInParent<CarView>();
		if (!(null != componentInParent))
		{
			return;
		}
		if (null != WheelEffectPrefab)
		{
			EffectToggleBase[] specialSparks = componentInParent.SkController.SpecialSparks;
			if (specialSparks != null && specialSparks.Length != 0)
			{
				for (int i = 0; i < specialSparks.Length; i++)
				{
					specialSparks[i].Active = false;
				}
				Transform parent = specialSparks[0].transform.parent;
				componentInParent.SkController.SpecialSparks = null;
				Object.Destroy(parent.gameObject);
			}
			else
			{
				componentInParent.SkController.SpecialSparks = null;
			}
		}
		componentInParent.carModel.GroundFrictionScale = 1f;
	}
}
