using UnityEngine;

[AddComponentMenu("CarLogic/Path Node")]
public class RacePathNode : MonoBehaviour
{
	public int Id;

	public bool ForceManul;

	public RacePathNode ParentNode;

	public RacePathNode MinorParentNode;

	public RacePathNode LeftNode;

	public RacePathNode RightNode;

	public int UserData;

	public float Distance;

	[HideInInspector]
	public bool useTransformNormal = true;

	[HideInInspector]
	public float CameraHightOffset;

	[HideInInspector]
	public Quaternion CameraAngle = Quaternion.identity;

	[HideInInspector]
	public Vector3 Up = Vector3.up;

	[HideInInspector]
	public Vector3 Forward = Vector3.forward;

	private bool isTwist;

	private static Vector3 vup = Vector3.up;

	public static int NodeLayer = -1;

	[HideInInspector]
	public bool IsTwist => isTwist;

	public void Awake()
	{
		if (NodeLayer == -1)
		{
			NodeLayer = LayerMask.NameToLayer("PathNode");
			if (NodeLayer == -1 || NodeLayer == 0)
			{
				NodeLayer = 13;
			}
		}
		base.gameObject.layer = NodeLayer;
		Forward = base.transform.forward;
		isTwist = Vector3.Dot(vup, Up) < 0.95f;
		if (useTransformNormal)
		{
			CameraAngle = base.transform.localRotation;
		}
	}

	public void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.red;
		if ((bool)LeftNode)
		{
			Gizmos.DrawLine(base.transform.position, LeftNode.transform.position);
		}
		if ((bool)RightNode)
		{
			Gizmos.DrawLine(base.transform.position, RightNode.transform.position);
			Gizmos.DrawWireSphere(base.transform.position, 0.3f);
		}
		if (!ParentNode)
		{
			Gizmos.DrawIcon(base.transform.position, "StartNode", allowScaling: true);
		}
		if (Up.sqrMagnitude != 0f)
		{
			Gizmos.color = Color.white;
			Gizmos.DrawRay(base.transform.position, Up);
		}
	}

	public void OnTriggerEnter(Collider other)
	{
		other.SendMessageUpwards("OnReachPathNode", this, SendMessageOptions.DontRequireReceiver);
	}

	public void OnTriggerExit(Collider other)
	{
		other.SendMessageUpwards("OnLeavePathNode", this, SendMessageOptions.DontRequireReceiver);
	}

	public static void SetNormal(RacePathNode node)
	{
		if (node == null)
		{
			return;
		}
		Transform transform = node.transform;
		int num = LayerMask.NameToLayer("Road");
		Vector3[] array = new Vector3[25]
		{
			new Vector3(-2f, 0f, -2f),
			new Vector3(-1f, 0f, -2f),
			new Vector3(0f, 0f, -2f),
			new Vector3(1f, 0f, -2f),
			new Vector3(2f, 0f, -2f),
			new Vector3(-2f, 0f, -1f),
			new Vector3(-1f, 0f, -1f),
			new Vector3(0f, -1f),
			new Vector3(1f, 0f, -1f),
			new Vector3(2f, 0f, -1f),
			new Vector3(-2f, 0f, 0f),
			new Vector3(-1f, 0f, 0f),
			new Vector3(0f, 0f, 0f),
			new Vector3(1f, 0f, 0f),
			new Vector3(2f, 0f, 0f),
			new Vector3(-2f, 0f, 1f),
			new Vector3(-1f, 0f, 1f),
			new Vector3(0f, 0f, 1f),
			new Vector3(1f, 0f, 1f),
			new Vector3(2f, 0f, 1f),
			new Vector3(-2f, 0f, 2f),
			new Vector3(-1f, 0f, 2f),
			new Vector3(0f, 0f, 2f),
			new Vector3(1f, 0f, 2f),
			new Vector3(2f, 0f, 2f)
		};
		Quaternion rotation = transform.rotation;
		Vector3 vector = rotation * new Vector3(0f, 0.5f, 0f);
		float num2 = 0.3f;
		Transform transform2 = new GameObject(node.name + "tmp").transform;
		transform2.rotation = rotation;
		transform2.position = transform.position;
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = transform2.TransformPoint(array[i] * num2 + vector);
		}
		if (Application.isPlaying)
		{
			Object.Destroy(transform2.gameObject);
		}
		else
		{
			Object.DestroyImmediate(transform2.gameObject);
		}
		int num3 = 0;
		Vector3 zero = Vector3.zero;
		Vector3 vector2 = -transform.up;
		for (int j = 0; j < array.Length; j++)
		{
			if (Physics.Raycast(array[j], vector2, out var hitInfo, 15f, 1 << num))
			{
				num3++;
				zero += hitInfo.normal;
			}
		}
		if (num3 != 0)
		{
			node.Up = zero / num3;
		}
		else
		{
			node.Up = -vector2;
		}
		Debug.Log(node.name + "  measure count: " + num3);
	}
}
