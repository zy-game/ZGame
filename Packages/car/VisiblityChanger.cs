using UnityEngine;

public class VisiblityChanger : MonoBehaviour
{
	public Transform Target;

	public void Start()
	{
		if (Target == null)
		{
			Target = base.transform;
		}
	}

	public void OnClick()
	{
		Target.gameObject.SetActive(!Target.gameObject.activeInHierarchy);
	}
}
