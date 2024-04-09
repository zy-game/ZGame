using UnityEngine;

public class TransShower : MonoBehaviour
{
	public Vector3 pos;

	public Vector3 rot;

	public void Update()
	{
		pos = base.transform.position;
		rot = base.transform.eulerAngles;
	}
}
