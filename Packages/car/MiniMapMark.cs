using UnityEngine;

public class MiniMapMark : MonoBehaviour
{
	private Quaternion _rotation = Quaternion.identity;

	private Vector3 _eulerAngle = Vector3.zero;

	private Vector3 vt;

	public float YOffset;

	public Transform _target;

	public bool AutoRotate;

	public void Start()
	{
		base.gameObject.layer = LayerMask.NameToLayer("MiniMap");
	}

	public void Update()
	{
		if (_target == null)
		{
			Object.Destroy(base.gameObject);
			return;
		}
		if (AutoRotate)
		{
			_eulerAngle.y = _target.localEulerAngles.y;
			_rotation.eulerAngles = _eulerAngle;
			base.transform.localRotation = _rotation;
		}
		vt = _target.localPosition;
		vt.y += YOffset;
		base.transform.localPosition = vt;
	}

	public void SetColor(Color c)
	{
		MeshRenderer componentInChildren = GetComponentInChildren<MeshRenderer>();
		if (componentInChildren != null)
		{
			componentInChildren.material.color = c;
		}
		else
		{
			Debug.LogWarning("UIGame_MiniMapMark: No MeshRender Found.");
		}
	}

	public void SetMaterial(Material material)
	{
		MeshRenderer componentInChildren = GetComponentInChildren<MeshRenderer>();
		if (componentInChildren != null)
		{
			componentInChildren.material = material;
		}
		else
		{
			Debug.LogWarning("UIGame_MiniMapMark: No MeshRender Found.");
		}
	}
}
