using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(BoxCollider))]
public class MapAnimationTrigger : MonoBehaviour
{
	private Collider _selfCollider;

	public Animation AnimObject;

	public LayerMask CheckLayerMask;

	public string EnterAnimName;

	public string StayAnimName;

	public string ExitAnimName;

	private void Awake()
	{
		_selfCollider = base.gameObject.GetComponent<BoxCollider>();
		if (null != _selfCollider)
		{
			_selfCollider.isTrigger = true;
		}
	}

	private bool CheckLayer(GameObject obj)
	{
		int num = (int)CheckLayerMask & (1 << obj.layer);
		return num == 1 << obj.layer;
	}

	private void OnTriggerEnter(Collider other)
	{
		if (CheckLayer(other.gameObject) && !string.IsNullOrEmpty(EnterAnimName) && null != AnimObject.GetClip(EnterAnimName))
		{
			AnimObject.Play(EnterAnimName);
		}
	}

	private void OnTriggerStay(Collider other)
	{
		if (CheckLayer(other.gameObject) && !string.IsNullOrEmpty(StayAnimName) && null != AnimObject.GetClip(StayAnimName))
		{
			AnimObject.Play(StayAnimName);
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (CheckLayer(other.gameObject) && !string.IsNullOrEmpty(ExitAnimName) && null != AnimObject.GetClip(ExitAnimName))
		{
			AnimObject.Play(ExitAnimName);
		}
	}
}
